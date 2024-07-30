using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HActLib.Internal;
using HActLib;
using MotionLib;
using MotionLibrary;

namespace DBGen
{
    public static class OEPropertyConversionModule
    {
        public static void Procedure()
        {
            string genDir = "motion/gen/property/";
            string genFilePath = genDir + "convertlist.txt";

            string output = "motion/bep/";

            if (!File.Exists(genFilePath))
                return;

            Console.WriteLine("------|OE PROPERTY TO DE BEP|-----");

            string[] dat = File.ReadAllLines(genFilePath);
            string property = dat[0];

            if(!File.Exists(property))
            {
                Console.WriteLine("Property.bin does not exist. aborting.");
                return;
            }

            OldEngineFormat oeProperty = OldEngineFormat.Read(property);

            for(int i = 1; i < dat.Length; i++)
            {
                string[] entryDat = dat[i].Split(' ');
                string exportName = "";

                if (entryDat.Length > 1)
                    exportName = entryDat[1] + ".bep";
                else
                    exportName = dat[i] + ".bep";

                string exportPath = Path.Combine(output, exportName);

                if (File.Exists(exportPath))
                {
                    Console.WriteLine(dat[i] + " already exists. ignoring...");
                    continue;
                }

                OEAnimEntry entry = oeProperty.Moves.Where(x => x.Name == entryDat[0]).FirstOrDefault();
                BEP converted = ConvertAnimEntryToBEP(entry);

                BEP.Write(converted, exportPath, Game.LADIW);

                Console.WriteLine("Converted " + entryDat[0]);
            }

            Console.WriteLine("------|OE PROPERTY TO DE BEP COMPLETE|-----");
        }

        private static BEP ConvertAnimEntryToBEP(OEAnimEntry entry)
        {
            BEP bep = new BEP();

            foreach (OEAnimProperty property in entry.Properties)
            {
                Node convertedNode = ConvertPropertyToNode(property, 16, Game.YK1, Game.LADIW);

                if (convertedNode != null)
                    bep.Nodes.Add(convertedNode);
            }

            return bep;
        }

        private static Node ConvertPropertyToNode(OEAnimProperty property, uint version, Game oeGame, Game game)
        {
            NodeElement deNode = null;

            switch (property.Type)
            {
                case OEPropertyType.VoiceAudio:
                    ushort voiceCuesheet = 0;

                    voiceCuesheet = RyuseModule.GetGVFighterIDForGame(game);

                    OEAnimPropertyVoiceSE oePropertyVoiceSE = property as OEAnimPropertyVoiceSE;
                    DEElementSE voiceSE = new DEElementSE();

                    voiceSE.ElementKind = Reflection.GetElementIDByName("e_auth_element_se", game);
                    voiceSE.CueSheet = voiceCuesheet;
                    voiceSE.SoundIndex = (byte)oePropertyVoiceSE.ID;
                    voiceSE.Unk = 128;

                    deNode = voiceSE;
                    break;

                case OEPropertyType.SEAudio:
                    OEAnimPropertySE oePropertySE = property as OEAnimPropertySE;
                    DEElementSE seNode = new DEElementSE();

                    seNode.ElementKind = Reflection.GetElementIDByName("e_auth_element_se", game);
                    seNode.CueSheet = oePropertySE.Cuesheet;
                    seNode.SoundIndex = (byte)(oePropertySE.ID + 1);

                    deNode = seNode;
                    break;

                case OEPropertyType.FollowupWindow:
                    DEElementFollowupWindow deFollowup = new DEElementFollowupWindow();
                    deFollowup.ElementKind = Reflection.GetElementIDByName("e_auth_element_battle_followup_window", game);

                    deNode = deFollowup;
                    break;

                case OEPropertyType.ControlLock:
                    DEElementControlLock deControl = new DEElementControlLock();
                    deControl.ElementKind = Reflection.GetElementIDByName("e_auth_element_battle_control_window", game);

                    deNode = deControl;
                    break;

                case OEPropertyType.Hitbox:
                    OEAnimPropertyHitbox oeHitbox = property as OEAnimPropertyHitbox;
                    DETimingInfoAttack attack = new DETimingInfoAttack();

                    attack.ElementKind = Reflection.GetElementIDByName("e_auth_element_battle_attack", game);

                    attack.Data.Damage = oeHitbox.Damage;

                    if (oeHitbox.HitboxLocation1.HasFlag(HitboxLocation1Flag.LeftHand))
                        attack.Data.Parts |= 4104;

                    if (oeHitbox.HitboxLocation1.HasFlag(HitboxLocation1Flag.RightHand))
                        attack.Data.Parts |= 2052;

                    if (oeHitbox.HitboxLocation1.HasFlag(HitboxLocation1Flag.LeftElbow))
                        attack.Data.Parts |= 5128;

                    if (oeHitbox.HitboxLocation1.HasFlag(HitboxLocation1Flag.RightElbow))
                        attack.Data.Parts |= 2564;

                    if (oeHitbox.HitboxLocation1.HasFlag(HitboxLocation1Flag.LeftKnee))
                        attack.Data.Parts |= 81952;

                    //Placeholder
                    attack.Data.Attributes = 1;
                    attack.Data.Power = 1;


                    deNode = attack;

                    break;

                case OEPropertyType.Muteki:
                    DETimingInfoMuteki invincibility = new DETimingInfoMuteki();
                    invincibility.ElementKind = Reflection.GetElementIDByName("e_auth_element_battle_muteki", game);

                    deNode = invincibility;

                    break;

            }

            if (deNode != null)
            {
                deNode.Start = property.Start;
                deNode.End = property.End;
                deNode.BEPDat.Guid2 = Guid.NewGuid();
                deNode.PlayType = ElementPlayType.Normal;
                deNode.UpdateTimingMode = 1;
            }

            return deNode;
        }
    }
}
