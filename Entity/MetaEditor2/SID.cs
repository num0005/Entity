using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using HaloMap.Map;
using HaloMap.Meta;
using HaloMap.Plugins;
using HaloMap.RealTimeHalo;

namespace entity.MetaEditor2
{    
    public partial class SID : BaseField
    {
        #region Fields
        //public int LineNumber;
        //private int mapIndex;
        //public int chunkOffset;
        //public int offsetInMap;
        //public string EntName = "Error in getting plugin element name";

        public int sidIndexer;
        const short WM_PAINT = 0x00f;
        private bool AddEvents = true;
        private bool isNulledOutReflexive = true;
        List<int> sidIndexerList = new List<int>(0);
        #endregion
        public SID(Meta meta, string iEntName, Map map, int iOffsetInChunk,int iLineNumber)
        {
            this.meta = meta;
            this.LineNumber = iLineNumber;
            this.chunkOffset = iOffsetInChunk;
            this.map = map;
            this.EntName = iEntName;
            this.size = 4;
            InitializeComponent();
            this.Size = this.PreferredSize;
            this.Dock = DockStyle.Top;
            this.Controls[0].Text = EntName;
            this.AutoSize = false;
        }

        public override void BaseField_Leave(object sender, EventArgs e)
        {
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(meta.MS);
            if (((WinMetaEditor)this.ParentForm).checkSelectionInCurrentTag())
                bw.BaseStream.Position = this.offsetInMap - meta.offset;

            bw.Write((short)this.sidIndexer);
            bw.Write((byte) 0);
            bw.Write((byte)map.Strings.Length[this.sidIndexer]);
            /*
            // Check for typed value
            SID sid = (SID)(sender);
            if (sid.comboBox1.Text != map.Strings.Name[sid.sidIndexer])
            {
                for (int i = 0; i < map.Strings.Name.Length; i++)
                    if (map.Strings.Name[i].ToLower() == sid.comboBox1.Text.ToLower())
                    {
                        sid.sidIndexer = i;
                        break;
                    }
                sid.comboBox1.Text = map.Strings.Name[sid.sidIndexer];
            }
            */
            //if (this.AutoSave)
            //    this.Save();
        }

        private void SIDLoader_DropDown(object sender, EventArgs e)
        {
            string tempSidString = ((System.Windows.Forms.ComboBox)sender).Text;
            SID sid = (SID)((Control)sender).Parent;

            MapForms.MapForm mf = (MapForms.MapForm)this.ParentForm.Owner;
            
            if (mf != null)
            {
                if (mf.sSwap == null)
                    mf.sSwap = new entity.MetaFuncs.MEStringsSelector(map.Strings.Name, ((Form)this.TopLevelControl).Owner);

                mf.sSwap.SelectedID = sid.sidIndexer;
                mf.sSwap.ShowDialog();
                //this.Enabled = true;

                if (sid.sidIndexer != mf.sSwap.SelectedID)
                {
                    sid.sidIndexer = mf.sSwap.SelectedID;
                    ((System.Windows.Forms.ComboBox)sender).SelectedIndex = -1;
                    ((System.Windows.Forms.ComboBox)sender).Text = map.Strings.Name[mf.sSwap.SelectedID];
                }

            }
            else
            {
                ((System.Windows.Forms.ComboBox)sender).Items.Clear();
                this.sidIndexerList.Clear();
                ((System.Windows.Forms.ComboBox)sender).Items.Add("");
                this.sidIndexerList.Add(0);
                for (int counter = 0; counter < map.Strings.Name.Length; counter++)
                {
                    if (map.Strings.Name[counter].Contains(tempSidString) == true)
                    {
                        ((System.Windows.Forms.ComboBox)sender).Items.Add(map.Strings.Name[counter]);
                        string xe = map.Strings.Name[counter];
                        this.sidIndexerList.Add(counter);
                        if (counter == sidIndexer)
                            ((System.Windows.Forms.ComboBox)sender).SelectedIndex = this.sidIndexerList.Count - 1;
                    }
                }
            }
        }       
 
        private void SIDLoader_DropDownClose(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.ComboBox)sender).SelectedIndex != -1)
            try
            {
                sidIndexer = sidIndexerList[((System.Windows.Forms.ComboBox)sender).SelectedIndex];
            }
            catch
            {
                sidIndexer = 0;
            }
        }

        void comboBox1_TextChanged(object sender, System.EventArgs e)
        {
        }

        public void Populate(int iOffset, bool useMemoryStream)
        {
            this.isNulledOutReflexive = false;
            System.IO.BinaryReader BR = new System.IO.BinaryReader(meta.MS);
            //set offsets
            BR.BaseStream.Position = iOffset + this.chunkOffset;
            BR.BaseStream.Position = iOffset + this.chunkOffset;
            this.offsetInMap = iOffset + this.chunkOffset;
            // If we need to read / save tag info directly to file...
            if (!useMemoryStream)
            {
                map.OpenMap(MapTypes.Internal);
                BR = map.BR;
                BR.BaseStream.Position = this.offsetInMap;
            }
            else
                this.offsetInMap += meta.offset;

            this.sidIndexer = BR.ReadInt16();
            byte tempnull = BR.ReadByte();
            byte sidLength = BR.ReadByte();

            // ...and then close the file once we are done!
            if (!useMemoryStream)
                map.CloseMap();

            try
            {
                string s = map.Strings.Name[this.sidIndexer];
                if (map.Strings.Length[this.sidIndexer] == sidLength)
                    ((System.Windows.Forms.ComboBox)this.Controls[1]).Text = s;
                else
                    ((System.Windows.Forms.ComboBox)this.Controls[1]).Text = "";
            }
            catch
            {
                ((System.Windows.Forms.ComboBox)this.Controls[1]).Text = "error reading sid";
            }
            if (AddEvents == true)
            {
                ((System.Windows.Forms.ComboBox)this.Controls[1]).TextChanged += new System.EventHandler(this.comboBox1_TextChanged);
                AddEvents = false;
            }
        }

        public override void Save()
        {
            if (this.isNulledOutReflexive == true)
                return;
            bool openedMap = false;
            if (map.isOpen == false)
            {
                map.OpenMap(MapTypes.Internal);
                openedMap = true;
            }
            try
            {
                map.BW.BaseStream.Position = this.offsetInMap;
                map.BW.Write((short)this.sidIndexer);
                map.BW.BaseStream.Position += 1;
                map.BW.Write((byte)map.Strings.Length[this.sidIndexer]);
            }
            catch
            {
                MessageBox.Show("Something is wrong with this Sid "+this.EntName+" Offset "+this.chunkOffset.ToString());
            }
            if (openedMap == true)
                map.CloseMap();
        }
        public void Poke()
        {
            uint Address = (uint)(this.offsetInMap + map.SelectedMeta.magic);
            try
            {
                uint StringID = (uint)(((ushort)this.sidIndexer) | ((byte)map.Strings.Length[this.sidIndexer] << 24));
                RTH_Imports.Poke(Address, StringID, 32);
            }
            catch
            {
                MessageBox.Show("Net: Something is wrong with this Sid " + this.EntName + " Offset " + this.chunkOffset.ToString());
            }

        }
        public void SetFocus(int LineToCheck)
        {
            if (this.LineNumber == LineToCheck)
                this.Focus();
        }

    }
}
