namespace Yazawa_Commander
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            attacksTree = new TreeView();
            icons = new ImageList(components);
            appToolstrip = new ToolStrip();
            fileTab = new ToolStripDropDownButton();
            newYHCToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            addDropdown = new ToolStripDropDownButton();
            attackYHCToolStripMenuItem = new ToolStripMenuItem();
            conditionsRootContext = new ContextMenuStrip(components);
            addConditionToolStripMenuItem = new ToolStripMenuItem();
            flagToolStripMenuItem = new ToolStripMenuItem();
            distanceToolStripMenuItem = new ToolStripMenuItem();
            attackGroupContext = new ContextMenuStrip(components);
            addToolStripMenuItem = new ToolStripMenuItem();
            attackCFCToolStripMenuItem = new ToolStripMenuItem();
            attackRPGToolStripMenuItem = new ToolStripMenuItem();
            attackGMTToolStripMenuItem = new ToolStripMenuItem();
            attackQuickstepToolStripMenuItem = new ToolStripMenuItem();
            attackSyncToolStripMenuItem = new ToolStripMenuItem();
            attackEmptyToolStripMenuItem = new ToolStripMenuItem();
            attackRangeCFCToolStripMenuItem = new ToolStripMenuItem();
            varPanel = new Elvis_Commander.DoubleBufferedTableLayoutPanel();
            attackContext = new ContextMenuStrip(components);
            moveUpToolStripMenuItem = new ToolStripMenuItem();
            moveDownToolStripMenuItem = new ToolStripMenuItem();
            conditionContext = new ContextMenuStrip(components);
            moveUpToolStripMenuItem1 = new ToolStripMenuItem();
            moveDownToolStripMenuItem1 = new ToolStripMenuItem();
            heatActionAttackContext = new ContextMenuStrip(components);
            moveUpToolStripMenuItem2 = new ToolStripMenuItem();
            moveDownToolStripMenuItem2 = new ToolStripMenuItem();
            addActorToolStripMenuItem = new ToolStripMenuItem();
            heatActionActorContext = new ContextMenuStrip(components);
            addToolStripMenuItem1 = new ToolStripMenuItem();
            flagToolStripMenuItem1 = new ToolStripMenuItem();
            heatActionConditionContext = new ContextMenuStrip(components);
            moveUpToolStripMenuItem3 = new ToolStripMenuItem();
            moveDownToolStripMenuItem3 = new ToolStripMenuItem();
            appToolstrip.SuspendLayout();
            conditionsRootContext.SuspendLayout();
            attackGroupContext.SuspendLayout();
            attackContext.SuspendLayout();
            conditionContext.SuspendLayout();
            heatActionAttackContext.SuspendLayout();
            heatActionActorContext.SuspendLayout();
            heatActionConditionContext.SuspendLayout();
            SuspendLayout();
            // 
            // attacksTree
            // 
            attacksTree.ImageIndex = 0;
            attacksTree.ImageList = icons;
            attacksTree.Location = new Point(15, 28);
            attacksTree.Name = "attacksTree";
            attacksTree.SelectedImageIndex = 0;
            attacksTree.Size = new Size(279, 410);
            attacksTree.TabIndex = 0;
            attacksTree.AfterSelect += attacksTree_AfterSelect;
            attacksTree.KeyDown += attacksTree_KeyDown;
            attacksTree.MouseDown += attacksTree_MouseDown;
            attacksTree.MouseUp += attacksTree_MouseUp;
            // 
            // icons
            // 
            icons.ColorDepth = ColorDepth.Depth8Bit;
            icons.ImageStream = (ImageListStreamer)resources.GetObject("icons.ImageStream");
            icons.TransparentColor = Color.Transparent;
            icons.Images.SetKeyName(0, "none.png");
            icons.Images.SetKeyName(1, "square.png");
            icons.Images.SetKeyName(2, "triangle.png");
            icons.Images.SetKeyName(3, "cross.png");
            icons.Images.SetKeyName(4, "circle.png");
            icons.Images.SetKeyName(5, "attack.png");
            icons.Images.SetKeyName(6, "condition.png");
            icons.Images.SetKeyName(7, "condition2.png");
            // 
            // appToolstrip
            // 
            appToolstrip.Items.AddRange(new ToolStripItem[] { fileTab, addDropdown });
            appToolstrip.Location = new Point(0, 0);
            appToolstrip.Name = "appToolstrip";
            appToolstrip.Size = new Size(754, 25);
            appToolstrip.TabIndex = 1;
            appToolstrip.Text = "appToolstrip";
            // 
            // fileTab
            // 
            fileTab.DisplayStyle = ToolStripItemDisplayStyle.Text;
            fileTab.DropDownItems.AddRange(new ToolStripItem[] { newYHCToolStripMenuItem, saveToolStripMenuItem, openToolStripMenuItem });
            fileTab.Image = (Image)resources.GetObject("fileTab.Image");
            fileTab.ImageTransparentColor = Color.Magenta;
            fileTab.Name = "fileTab";
            fileTab.Size = new Size(38, 22);
            fileTab.Text = "File";
            // 
            // newYHCToolStripMenuItem
            // 
            newYHCToolStripMenuItem.Name = "newYHCToolStripMenuItem";
            newYHCToolStripMenuItem.Size = new Size(180, 22);
            newYHCToolStripMenuItem.Text = "New EHC";
            newYHCToolStripMenuItem.Click += newYHCToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(180, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(180, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // addDropdown
            // 
            addDropdown.DisplayStyle = ToolStripItemDisplayStyle.Text;
            addDropdown.DropDownItems.AddRange(new ToolStripItem[] { attackYHCToolStripMenuItem });
            addDropdown.Image = (Image)resources.GetObject("addDropdown.Image");
            addDropdown.ImageTransparentColor = Color.Magenta;
            addDropdown.Name = "addDropdown";
            addDropdown.Size = new Size(42, 22);
            addDropdown.Text = "Add";
            // 
            // attackYHCToolStripMenuItem
            // 
            attackYHCToolStripMenuItem.Name = "attackYHCToolStripMenuItem";
            attackYHCToolStripMenuItem.Size = new Size(142, 22);
            attackYHCToolStripMenuItem.Text = "Attack (EHC)";
            attackYHCToolStripMenuItem.Click += attackYHCToolStripMenuItem_Click;
            // 
            // conditionsRootContext
            // 
            conditionsRootContext.Items.AddRange(new ToolStripItem[] { addConditionToolStripMenuItem });
            conditionsRootContext.Name = "attackContextMenu";
            conditionsRootContext.Size = new Size(97, 26);
            // 
            // addConditionToolStripMenuItem
            // 
            addConditionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { flagToolStripMenuItem, distanceToolStripMenuItem });
            addConditionToolStripMenuItem.Name = "addConditionToolStripMenuItem";
            addConditionToolStripMenuItem.Size = new Size(96, 22);
            addConditionToolStripMenuItem.Text = "Add";
            // 
            // flagToolStripMenuItem
            // 
            flagToolStripMenuItem.Name = "flagToolStripMenuItem";
            flagToolStripMenuItem.Size = new Size(119, 22);
            flagToolStripMenuItem.Text = "Flag";
            flagToolStripMenuItem.Click += flagToolStripMenuItem_Click;
            // 
            // distanceToolStripMenuItem
            // 
            distanceToolStripMenuItem.Name = "distanceToolStripMenuItem";
            distanceToolStripMenuItem.Size = new Size(119, 22);
            distanceToolStripMenuItem.Text = "Distance";
            // 
            // attackGroupContext
            // 
            attackGroupContext.Items.AddRange(new ToolStripItem[] { addToolStripMenuItem });
            attackGroupContext.Name = "attackGroupContext";
            attackGroupContext.Size = new Size(97, 26);
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { attackCFCToolStripMenuItem, attackRPGToolStripMenuItem, attackGMTToolStripMenuItem, attackQuickstepToolStripMenuItem, attackSyncToolStripMenuItem, attackEmptyToolStripMenuItem, attackRangeCFCToolStripMenuItem });
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new Size(96, 22);
            addToolStripMenuItem.Text = "Add";
            // 
            // attackCFCToolStripMenuItem
            // 
            attackCFCToolStripMenuItem.Name = "attackCFCToolStripMenuItem";
            attackCFCToolStripMenuItem.Size = new Size(177, 22);
            attackCFCToolStripMenuItem.Text = "Attack (CFC)";
            attackCFCToolStripMenuItem.Click += attackCFCToolStripMenuItem_Click;
            // 
            // attackRPGToolStripMenuItem
            // 
            attackRPGToolStripMenuItem.Name = "attackRPGToolStripMenuItem";
            attackRPGToolStripMenuItem.Size = new Size(177, 22);
            attackRPGToolStripMenuItem.Text = "Attack (RPG)";
            attackRPGToolStripMenuItem.Click += attackRPGToolStripMenuItem_Click;
            // 
            // attackGMTToolStripMenuItem
            // 
            attackGMTToolStripMenuItem.Name = "attackGMTToolStripMenuItem";
            attackGMTToolStripMenuItem.Size = new Size(177, 22);
            attackGMTToolStripMenuItem.Text = "Attack (GMT)";
            attackGMTToolStripMenuItem.Click += attackGMTToolStripMenuItem_Click;
            // 
            // attackQuickstepToolStripMenuItem
            // 
            attackQuickstepToolStripMenuItem.Name = "attackQuickstepToolStripMenuItem";
            attackQuickstepToolStripMenuItem.Size = new Size(177, 22);
            attackQuickstepToolStripMenuItem.Text = "Attack (Quickstep)";
            attackQuickstepToolStripMenuItem.Click += attackQuickstepToolStripMenuItem_Click;
            // 
            // attackSyncToolStripMenuItem
            // 
            attackSyncToolStripMenuItem.Name = "attackSyncToolStripMenuItem";
            attackSyncToolStripMenuItem.Size = new Size(177, 22);
            attackSyncToolStripMenuItem.Text = "Attack (Sync)";
            attackSyncToolStripMenuItem.Click += attackSyncToolStripMenuItem_Click;
            // 
            // attackEmptyToolStripMenuItem
            // 
            attackEmptyToolStripMenuItem.Name = "attackEmptyToolStripMenuItem";
            attackEmptyToolStripMenuItem.Size = new Size(177, 22);
            attackEmptyToolStripMenuItem.Text = "Attack (Empty)";
            attackEmptyToolStripMenuItem.Click += attackEmptyToolStripMenuItem_Click;
            // 
            // attackRangeCFCToolStripMenuItem
            // 
            attackRangeCFCToolStripMenuItem.Name = "attackRangeCFCToolStripMenuItem";
            attackRangeCFCToolStripMenuItem.Size = new Size(177, 22);
            attackRangeCFCToolStripMenuItem.Text = "Attack (Range CFC)";
            attackRangeCFCToolStripMenuItem.Click += attackRangeCFCToolStripMenuItem_Click;
            // 
            // varPanel
            // 
            varPanel.AutoScroll = true;
            varPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.InsetDouble;
            varPanel.ColumnCount = 2;
            varPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            varPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 317F));
            varPanel.Location = new Point(300, 31);
            varPanel.Name = "varPanel";
            varPanel.RowCount = 3;
            varPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            varPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            varPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            varPanel.Size = new Size(442, 407);
            varPanel.TabIndex = 2;
            // 
            // attackContext
            // 
            attackContext.Items.AddRange(new ToolStripItem[] { moveUpToolStripMenuItem, moveDownToolStripMenuItem });
            attackContext.Name = "attackContext";
            attackContext.Size = new Size(139, 48);
            // 
            // moveUpToolStripMenuItem
            // 
            moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            moveUpToolStripMenuItem.Size = new Size(138, 22);
            moveUpToolStripMenuItem.Text = "Move Up";
            moveUpToolStripMenuItem.Click += moveUpToolStripMenuItem_Click;
            // 
            // moveDownToolStripMenuItem
            // 
            moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            moveDownToolStripMenuItem.Size = new Size(138, 22);
            moveDownToolStripMenuItem.Text = "Move Down";
            moveDownToolStripMenuItem.Click += moveDownToolStripMenuItem_Click;
            // 
            // conditionContext
            // 
            conditionContext.Items.AddRange(new ToolStripItem[] { moveUpToolStripMenuItem1, moveDownToolStripMenuItem1 });
            conditionContext.Name = "conditionContext";
            conditionContext.Size = new Size(139, 48);
            // 
            // moveUpToolStripMenuItem1
            // 
            moveUpToolStripMenuItem1.Name = "moveUpToolStripMenuItem1";
            moveUpToolStripMenuItem1.Size = new Size(138, 22);
            moveUpToolStripMenuItem1.Text = "Move Up";
            moveUpToolStripMenuItem1.Click += moveUpToolStripMenuItem1_Click;
            // 
            // moveDownToolStripMenuItem1
            // 
            moveDownToolStripMenuItem1.Name = "moveDownToolStripMenuItem1";
            moveDownToolStripMenuItem1.Size = new Size(138, 22);
            moveDownToolStripMenuItem1.Text = "Move Down";
            moveDownToolStripMenuItem1.Click += moveDownToolStripMenuItem1_Click;
            // 
            // heatActionAttackContext
            // 
            heatActionAttackContext.Items.AddRange(new ToolStripItem[] { moveUpToolStripMenuItem2, moveDownToolStripMenuItem2, addActorToolStripMenuItem });
            heatActionAttackContext.Name = "heatActionAttackContext";
            heatActionAttackContext.Size = new Size(139, 70);
            // 
            // moveUpToolStripMenuItem2
            // 
            moveUpToolStripMenuItem2.Name = "moveUpToolStripMenuItem2";
            moveUpToolStripMenuItem2.Size = new Size(138, 22);
            moveUpToolStripMenuItem2.Text = "Move Up";
            moveUpToolStripMenuItem2.Click += moveUpToolStripMenuItem2_Click;
            // 
            // moveDownToolStripMenuItem2
            // 
            moveDownToolStripMenuItem2.Name = "moveDownToolStripMenuItem2";
            moveDownToolStripMenuItem2.Size = new Size(138, 22);
            moveDownToolStripMenuItem2.Text = "Move Down";
            moveDownToolStripMenuItem2.Click += moveDownToolStripMenuItem2_Click;
            // 
            // addActorToolStripMenuItem
            // 
            addActorToolStripMenuItem.Name = "addActorToolStripMenuItem";
            addActorToolStripMenuItem.Size = new Size(138, 22);
            addActorToolStripMenuItem.Text = "Add Actor";
            addActorToolStripMenuItem.Click += addActorToolStripMenuItem_Click;
            // 
            // heatActionActorContext
            // 
            heatActionActorContext.Items.AddRange(new ToolStripItem[] { addToolStripMenuItem1 });
            heatActionActorContext.Name = "heatActionActorContext";
            heatActionActorContext.Size = new Size(97, 26);
            // 
            // addToolStripMenuItem1
            // 
            addToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { flagToolStripMenuItem1 });
            addToolStripMenuItem1.Name = "addToolStripMenuItem1";
            addToolStripMenuItem1.Size = new Size(96, 22);
            addToolStripMenuItem1.Text = "Add";
            // 
            // flagToolStripMenuItem1
            // 
            flagToolStripMenuItem1.Name = "flagToolStripMenuItem1";
            flagToolStripMenuItem1.Size = new Size(127, 22);
            flagToolStripMenuItem1.Text = "Condition";
            flagToolStripMenuItem1.Click += flagToolStripMenuItem1_Click;
            // 
            // heatActionConditionContext
            // 
            heatActionConditionContext.Items.AddRange(new ToolStripItem[] { moveUpToolStripMenuItem3, moveDownToolStripMenuItem3 });
            heatActionConditionContext.Name = "heatActionCondition";
            heatActionConditionContext.Size = new Size(139, 48);
            // 
            // moveUpToolStripMenuItem3
            // 
            moveUpToolStripMenuItem3.Name = "moveUpToolStripMenuItem3";
            moveUpToolStripMenuItem3.Size = new Size(138, 22);
            moveUpToolStripMenuItem3.Text = "Move Up";
            moveUpToolStripMenuItem3.Click += moveUpToolStripMenuItem3_Click;
            // 
            // moveDownToolStripMenuItem3
            // 
            moveDownToolStripMenuItem3.Name = "moveDownToolStripMenuItem3";
            moveDownToolStripMenuItem3.Size = new Size(138, 22);
            moveDownToolStripMenuItem3.Text = "Move Down";
            moveDownToolStripMenuItem3.Click += moveDownToolStripMenuItem3_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(754, 450);
            Controls.Add(varPanel);
            Controls.Add(appToolstrip);
            Controls.Add(attacksTree);
            Name = "Main";
            Text = "Elvis Commander";
            appToolstrip.ResumeLayout(false);
            appToolstrip.PerformLayout();
            conditionsRootContext.ResumeLayout(false);
            attackGroupContext.ResumeLayout(false);
            attackContext.ResumeLayout(false);
            conditionContext.ResumeLayout(false);
            heatActionAttackContext.ResumeLayout(false);
            heatActionActorContext.ResumeLayout(false);
            heatActionConditionContext.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView attacksTree;
        private ToolStrip appToolstrip;
        private ToolStripDropDownButton fileTab;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem addConditionToolStripMenuItem;
        private ToolStripMenuItem flagToolStripMenuItem;
        private ToolStripMenuItem distanceToolStripMenuItem;
        public ContextMenuStrip conditionsRootContext;
        private ToolStripDropDownButton addDropdown;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem attackCFCToolStripMenuItem;
        private ToolStripMenuItem attackRPGToolStripMenuItem;
        public ContextMenuStrip attackGroupContext;
        private ToolStripMenuItem openToolStripMenuItem;
        private ImageList icons;
        private ToolStripMenuItem attackGMTToolStripMenuItem;
        private ToolStripMenuItem attackQuickstepToolStripMenuItem;
        private ToolStripMenuItem attackSyncToolStripMenuItem;
        private ToolStripMenuItem moveUpToolStripMenuItem;
        private ToolStripMenuItem moveDownToolStripMenuItem;
        public ContextMenuStrip attackContext;
        public ContextMenuStrip conditionContext;
        private ToolStripMenuItem moveUpToolStripMenuItem1;
        private ToolStripMenuItem moveDownToolStripMenuItem1;
        private ToolStripMenuItem newYHCToolStripMenuItem;
        private ToolStripMenuItem addActorToolStripMenuItem;
        private ToolStripMenuItem attackYHCToolStripMenuItem;
        public ContextMenuStrip heatActionAttackContext;
        public ContextMenuStrip heatActionActorContext;
        private ToolStripMenuItem addToolStripMenuItem1;
        private ToolStripMenuItem flagToolStripMenuItem1;
        private ToolStripMenuItem moveUpToolStripMenuItem2;
        private ToolStripMenuItem moveDownToolStripMenuItem2;
        private ToolStripMenuItem moveUpToolStripMenuItem3;
        private ToolStripMenuItem moveDownToolStripMenuItem3;
        public ContextMenuStrip heatActionConditionContext;
        private ToolStripMenuItem attackEmptyToolStripMenuItem;
        private ToolStripMenuItem attackRangeCFCToolStripMenuItem;
        private Elvis_Commander.DoubleBufferedTableLayoutPanel varPanel;
    }
}