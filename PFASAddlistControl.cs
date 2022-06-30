using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using HedgeTek.PFASDatabaseDLL;
using HedgeTek.PFASErrorHandler;
//using log4net;
//using log4net.Config;
using HedgeTek.PFASUtility;
using Microsoft.VisualBasic;
using HedgeTek.PFASCentralDLL;

/*
 *Modified by Rajarshi on May 04 2016 - Error Handler has been deployed.
 *Modified by Rajarshi on May 05 2016 - Error Handler has been revoked from UserControl_Resize(). 
 *Modified by Rajarshi on Jul 07 2017 - Session Timeout has been incorporated.
 * */


namespace HedgeTek.Addlist
{
    public enum SortType
    {
        NotSorted = 0,
        Ascending = 1,
        Descending = 2
    }

    public enum ListType
    {
        Source = 0,
        Destination = 1
    }

    public enum Appearance
    {
        Flat = 0,
        _3D = 1
    }

    public enum UpDownDirection
    {
        up = -1,
        down = 1
    }

    public enum MoveDirection
    {
        MoveToLeft = 1,
        MoveToRight = 2
    }

    public enum ShiftConstants
    {
        vbShiftMask = 1,
        vbCtrlMask = 2,
        vbAltMask = 4
    }

    public partial class PFASAddListControl : UserControl
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(PFASAddListControl));

        //Default Property Values:
        const string m_def_QueryString = "";
        const string m_def_BoundColumn = "";
        const SortType m_def_SortType = SortType.NotSorted;
        const string m_def_SortCol = "";
        const string m_def_tag = "";

        //Property Variables:
        private string m_Src_QueryString = "";
        private string m_Dest_QueryString = "";
        private bool m_IsDirty = false;
        private bool m_SkipReset = false;
        private bool m_SkipDirty = false;
        private string m_Src_BoundColumn = "";
        private string m_Dest_BoundColumn = "";
        private int m_Src_ListIndex = 0;
        private int m_Dest_ListIndex = 0;
        private string m_Src_SortCol = "";
        private string m_Dest_SortCol = "";
        private SortType m_Src_SortType = SortType.NotSorted;
        private SortType m_Dest_SortType = SortType.NotSorted;
        //private ListType m_ListType = ListType.Source;
        private List<string> SrcItemCollection = null;
        private List<string> DestItemCollection = null;
        private List<dynamic> SrcItemDataCollection = null;
        private List<dynamic> DestItemDataCollection = null;
        private bool m_Allowsequencing = false;
        private string m_Tag = "";
        private bool Flag = false;

        int indexOfItemUnderMouseToDrag = 0;
        Rectangle dragBoxFromMouseDown;

        

        //Event Declarations:
        public event KeyDownCustomEventHandler KeyDownCustom;
        public delegate void KeyDownCustomEventHandler(Keys KeyCode, ShiftConstants Shift);

        public event KeyPressCustomEventHandler KeyPressCustom;
        public delegate void KeyPressCustomEventHandler(int KeyAscii);

        public event KeyUpCustomEventHandler KeyUpCustom;
        public delegate void KeyUpCustomEventHandler(Keys KeyCode, ShiftConstants Shift);

        public event MouseDownCustomEventHandler MouseDownCustom;
        public delegate void MouseDownCustomEventHandler(int button, int Shift, float X, float Y);

        public event MouseMoveCustomEventHandler MouseMoveCustom;
        public delegate void MouseMoveCustomEventHandler(int button, int Shift, float X, float Y);

        public event MouseUpCustomEventHandler MouseUpCustom;
        public delegate void MouseUpCustomEventHandler(int button, int Shift, float X, float Y);

        public event ResizeCustomEventHandler ResizeCustom;
        public delegate void ResizeCustomEventHandler();

        public event AfterResetEventHandler AfterReset;
        public delegate void AfterResetEventHandler();

        public event OnButtonClickEventHandler OnButtonClick;
        public delegate void OnButtonClickEventHandler(int Index);


        public PFASAddListControl()
        {
            InitializeComponent();

            UserControl_Initialize();
            UserControl_InitProperties();

            SrcItemCollection = new List<string>();
            DestItemCollection = new List<string>();
            SrcItemDataCollection = new List<dynamic>();
            DestItemDataCollection = new List<dynamic>();

            this.cmdMovefields0.Click += new EventHandler(this.cmdMovefields_Click);
            this.cmdMovefields1.Click += new EventHandler(this.cmdMovefields_Click);
            this.cmdMovefields2.Click += new EventHandler(this.cmdMovefields_Click);
            this.cmdMovefields3.Click += new EventHandler(this.cmdMovefields_Click);

            this.cmdUpDown0.Click += new EventHandler(this.cmdUpDown_Click);
            this.cmdUpDown1.Click += new EventHandler(this.cmdUpDown_Click);

            this.lstSource.KeyDown += new KeyEventHandler(this.lstSource_KeyDown);
            this.lstSource.KeyPress += new KeyPressEventHandler(this.lstSource_KeyPress);
            this.lstSource.KeyUp += new KeyEventHandler(this.lstSource_KeyUp);

            this.lstDestination.KeyDown += new KeyEventHandler(this.lstSource_KeyDown);
            this.lstDestination.KeyPress += new KeyPressEventHandler(this.lstSource_KeyPress);
            this.lstDestination.KeyUp += new KeyEventHandler(this.lstSource_KeyUp);

            this.lstSource.MouseDown += new MouseEventHandler(this.lstSource_MouseDown);
            this.lstSource.MouseUp += new MouseEventHandler(this.lstSource_MouseUp);
            this.lstSource.MouseMove += new MouseEventHandler(this.lstSource_MouseMove);

            this.lstDestination.MouseDown += new MouseEventHandler(this.lstDestination_MouseDown);
            this.lstDestination.MouseUp += new MouseEventHandler(this.lstDestination_MouseUp);
            this.lstDestination.MouseMove += new MouseEventHandler(this.lstDestination_MouseMove);
        }

        public void AddItem(ListType NewListType, string Item, dynamic Index = null)
        {
            try
            {
                if (NewListType == ListType.Source)
                {
                    if (Index == null)
                    {
                        lstSource.Items.Add(new ListBoxItem(Item));
                    }
                    else
                    {
                        lstSource.Items.Add(new ListBoxItem(Item, Index));
                    }
                    SrcItemCollection.Add(Item);
                    Selected(ListType.Source, 0, true);
                }
                else if (NewListType == ListType.Destination)
                {
                    if (Index == null)
                    {
                        lstDestination.Items.Add(new ListBoxItem(Item));
                    }
                    else
                    {
                        lstDestination.Items.Add(new ListBoxItem(Item, Index));
                    }
                    DestItemCollection.Add(Item);
                    Selected(ListType.Destination, 0, true);
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        [Bindable(true), Category(""), Description("")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                lstSource.BackColor = value;
                lstDestination.BackColor = value;
            }
        }

        public void Clear()
        {
            lstSource.Items.Clear();
            lstDestination.Items.Clear();
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                bool New_Enabled = value;
                base.Enabled = New_Enabled;

                lstSource.BackColor = (!New_Enabled) ? SystemColors.ButtonFace : SystemColors.Window;
                lstDestination.BackColor = (!New_Enabled) ? SystemColors.ButtonFace : SystemColors.Window;

                cmdMovefields0.Enabled = New_Enabled;
                cmdMovefields1.Enabled = New_Enabled;
                cmdMovefields2.Enabled = New_Enabled;
                cmdMovefields3.Enabled = New_Enabled;
            }
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                lstSource.Font = value;
                lstDestination.Font = value;
                lblCaption0.Font = value;
                lblCaption1.Font = value;
            }
        }

        [Bindable(true), Category(""), Description("")]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                base.ForeColor = value;
                lstSource.ForeColor = value;
                lstDestination.ForeColor = value;
            }
        }

        [Browsable(false)]
        public long hwnd
        {
            get { return lstSource.Handle.ToInt64(); }
        }

        public long Src_ItemData(int Index)
        {
            long result = 0;

            try
            {
                result = ((ListBoxItem)lstSource.Items[Index]).Index;
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return result;
        }

        public long Src_ItemData(int Index, long New_ItemData)
        {
            try
            {
                ((ListBoxItem)lstSource.Items[Index]).Index = New_ItemData;

                SrcItemDataCollection.Add(New_ItemData);
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return New_ItemData;
        }

        public long Dest_ItemData(int Index)
        {
            long result = 0;

            try
            {
                result = ((ListBoxItem)lstDestination.Items[Index]).Index;
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return result;
        }

        public long Dest_ItemData(int Index, long New_ItemData)
        {
            try
            {
                ((ListBoxItem)lstDestination.Items[Index]).Index = New_ItemData;

                DestItemDataCollection.Add(New_ItemData);
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return New_ItemData;
        }
        
        private void cmdMovefields_Click(object sender, EventArgs e)
        {
            int Index = -1;
            int I = 0;
            //int J = 0;
            int k = 0;
            //int count = 0;
            bool bToSkipSessionTimeOutCheck = false;
            string strParentFormNm = "";

            try
            {
                if (this.ParentForm != null)
                {
                    strParentFormNm = this.ParentForm.Name;
                }

                if (CCentralFunction.CCF_HasSessionTimedOut(ref bToSkipSessionTimeOutCheck))
                {
                    return;
                }



                Button btnClick = (Button)sender;

                if (btnClick.Equals(cmdMovefields0))
                {
                    Index = 0;
                }
                else if (btnClick.Equals(cmdMovefields1))
                {
                    Index = 1;
                }
                else if (btnClick.Equals(cmdMovefields2))
                {
                    Index = 2;
                }
                else if (btnClick.Equals(cmdMovefields3))
                {
                    Index = 3;
                }

                switch (Index)
                {
                    case 0:
                        {
                            for (I = 0; I <= lstSource.Items.Count - 1; I++)
                            {
                                if (lstSource.GetSelected(I) == true)
                                {
                                    lstDestination.Items.Add(new ListBoxItem(((ListBoxItem)lstSource.Items[I]).Item, ((ListBoxItem)lstSource.Items[I]).Index));

                                    k = k + 1;
                                    if (k == GetSelCount(lstSource))
                                    {
                                        break;
                                    }
                                }
                            }

                            for (I = 0; I <= lstSource.Items.Count - 1; I++)
                            {
                                if (lstSource.GetSelected(I))
                                {
                                    RemoveItem(ListType.Source, I);
                                    I = I - 1;

                                    if (GetSelCount(lstSource) == 0)
                                    {
                                        if (lstSource.Items.Count > 0)
                                        {
                                            Selected(ListType.Source, 0, true);
                                        }
                                        else if (lstSource.Items.Count == 0)
                                        {
                                            Selected(ListType.Destination, 0, true);
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case 1:
                        {
                            for (I = 0; I <= lstSource.Items.Count - 1; I++)
                            {
                                lstDestination.Items.Add(new ListBoxItem(((ListBoxItem)lstSource.Items[I]).Item, ((ListBoxItem)lstSource.Items[I]).Index));
                            }

                            for (I = 0; I <= lstSource.Items.Count - 1; I++)
                            {
                                RemoveItem(ListType.Source, I);
                                I = I - 1;
                                if (lstSource.Items.Count == 0)
                                {
                                    break;
                                }
                            }

                            cmdMovefields1.Enabled = false;
                            Selected(ListType.Destination, 0, true);
                            break;
                        }
                    case 2:
                        {
                            for (I = 0; I <= lstDestination.Items.Count - 1; I++)
                            {
                                if (lstDestination.GetSelected(I) == true)
                                {
                                    lstSource.Items.Add(new ListBoxItem(((ListBoxItem)lstDestination.Items[I]).Item, ((ListBoxItem)lstDestination.Items[I]).Index));

                                    k = k + 1;
                                    if (k == GetSelCount(lstDestination))
                                    {
                                        break;
                                    }
                                }
                            }

                            for (I = 0; I <= lstDestination.Items.Count - 1; I++)
                            {
                                if (lstDestination.GetSelected(I))
                                {
                                    RemoveItem(ListType.Destination, I);
                                    I = I - 1;
                                    if (GetSelCount(lstDestination) == 0)
                                    {
                                        if (lstDestination.Items.Count > 0)
                                        {
                                            Selected(ListType.Destination, 0, true);
                                        }
                                        else if (lstDestination.Items.Count == 0)
                                        {
                                            Selected(ListType.Source, 0, true);
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case 3:
                        {
                            for (I = 0; I <= lstDestination.Items.Count - 1; I++)
                            {
                                lstSource.Items.Add(new ListBoxItem(((ListBoxItem)lstDestination.Items[I]).Item, ((ListBoxItem)lstDestination.Items[I]).Index));
                            }

                            for (I = 0; I <= lstDestination.Items.Count - 1; I++)
                            {
                                RemoveItem(ListType.Destination, I);
                                I = I - 1;
                                if (lstDestination.Items.Count == 0)
                                {
                                    break;
                                }
                            }

                            cmdMovefields3.Enabled = false;
                            Selected(ListType.Source, 0, true);
                            break;
                        }
                }

                if (OnButtonClick != null)
                {
                    OnButtonClick.Invoke(Index);
                }

                if (!m_SkipDirty)
                {
                    m_IsDirty = true;
                }

                EnableDisableAddlist();




            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                if (bToSkipSessionTimeOutCheck == false)
                {
                    CCentralFunction.CCF_SaveLastActivityDateTime(this.Name, "", "cmdMovefields_Click", strParentFormNm);
                }
            }

        }

        private void cmdUpDown_Click(object sender, EventArgs e)
        {
            int Index = -1;
            bool bToSkipSessionTimeOutCheck = false;
            string strParentFormNm = "";
            try
            {
                Button btnUpDown = (Button)sender;

                if (this.ParentForm != null)
                {
                    strParentFormNm = this.ParentForm.Name;
                }

                if (CCentralFunction.CCF_HasSessionTimedOut(ref bToSkipSessionTimeOutCheck))
                {
                    return;
                }

                if (btnUpDown.Equals(cmdUpDown0))
                {
                    Index = 0;
                }
                else if (btnUpDown.Equals(cmdUpDown1))
                {
                    Index = 1;
                }

                Sequencing(Convert.ToBoolean(Index) ? -1 : 1);
                EnableDisableUpDown();
            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                if (bToSkipSessionTimeOutCheck == false)
                {
                    CCentralFunction.CCF_SaveLastActivityDateTime(this.Name, "", "cmdUpDown_Click", strParentFormNm);
                }
            }
        }

        private void lstDestination_Click(object sender, EventArgs e)
        {
            int I = 0;
            bool bToSkipSessionTimeOutCheck = false;
            string strParentFormNm = "";

            try
            {
                if (this.ParentForm != null)
                {
                    strParentFormNm = this.ParentForm.Name;
                }

                if (CCentralFunction.CCF_HasSessionTimedOut(ref bToSkipSessionTimeOutCheck))
                {
                    return;
                }

                cmdUpDown0.Enabled = !lstDestination.GetSelected(0);
                cmdUpDown1.Enabled = !lstDestination.GetSelected(lstDestination.Items.Count - 1);

                if (Flag == false)
                {
                    for (I = 0; I <= lstSource.Items.Count - 1; I++)
                    {
                        if (lstSource.GetSelected(I) == true)
                        {
                            Flag = true;
                            lstSource.SetSelected(I, false);
                        }
                    }
                }
                else
                {
                    Flag = false;
                }





            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
            finally
            {
                if (bToSkipSessionTimeOutCheck == false)
                {
                    CCentralFunction.CCF_SaveLastActivityDateTime(this.Name, "", "lstDestination_Click", strParentFormNm);
                }
            }
        }

        private void lstDestination_DragDrop(object sender, DragEventArgs e)
        {
            //VB code needs to be implemented
            //Source As Control, X As Single, Y As Single
            //If Source.Name = "lstSource" Then
            //    cmdMovefields_Click 0
            //End If
        }

        private void lstDestination_MouseMove(object sender, MouseEventArgs e)
        {
            //button As Integer, Shift As Integer, X As Single, Y As Single
            /*
            With lstDestination    
                If button Then  ' Signal a Drag operation.            
                    ' Set the drag icon
                    If .SelCount > 1 Then
                       .DragIcon = imglst.ListImages.Item(5).Picture
                    Else
                       .DragIcon = imglst.ListImages(4).Picture
                    End If            
                    .Drag vbBeginDrag          
                End If        
            End With
            */

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    //DragDropEffects dropEffect = lstSource.DoDragDrop(lstSource.Items[indexOfItemUnderMouseToDrag], DragDropEffects.All | DragDropEffects.Link);

                    DragDropEffects dropEffect = lstSource.DoDragDrop(lstSource.Items[indexOfItemUnderMouseToDrag], DragDropEffects.All);
                    cmdMovefields_Click(cmdMovefields2, new EventArgs());


                    /*
                    // If the drag operation was a move then remove the item.
                    if (dropEffect == DragDropEffects.Move)
                    {
                        lstSource.Items.RemoveAt(indexOfItemUnderMouseToDrag);

                        // Selects the previous item in the list as long as the list has an item.
                        if (indexOfItemUnderMouseToDrag > 0)
                            lstSource.SelectedIndex = indexOfItemUnderMouseToDrag - 1;
                        
                        else if (ListDragSource.Items.Count > 0)
                            // Selects the first item.
                            lstSource.SelectedIndex = 0;
                    }
                    */
                }
            }

            if (!m_SkipDirty)
            {
                m_IsDirty = true;
            }
        }

        private void lstSource_Click(object sender, EventArgs e)
        {
            int I = 0;

            bool bToSkipSessionTimeOutCheck = false;

            string strParentFormNm = "";

            try
            {
                if (this.ParentForm != null)
                {
                    strParentFormNm = this.ParentForm.Name;
                }

                if (CCentralFunction.CCF_HasSessionTimedOut(ref bToSkipSessionTimeOutCheck))
                {
                    return;
                }

                if (Flag == false)
                {
                    for (I = 0; I <= lstDestination.Items.Count - 1; I++)
                    {
                        if (lstDestination.GetSelected(I) == true)
                        {
                            Flag = true;
                            lstDestination.SetSelected(I, false);
                        }
                    }
                }
                else
                {
                    Flag = false;
                }

                EnableDisableUpDown();


            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
            finally
            {
                if (bToSkipSessionTimeOutCheck == false)
                {
                    CCentralFunction.CCF_SaveLastActivityDateTime(this.Name, "", "lstSource_Click", strParentFormNm);
                }
            }
        }

        private void lstSource_DragDrop(object sender, DragEventArgs e)
        {
            //VB code needs to be implemented
            //Source As Control, X As Single, Y As Single
            //If Source.Name = "lstDestination" Then
            //    cmdMovefields_Click 2
            //End If
        }
        
        private void lstSource_KeyDown(object sender, KeyEventArgs e)
        {
            Keys KeyCode = e.KeyCode;
            ShiftConstants Shift = ShiftConstants.vbShiftMask;

            try
            {
                if (e.Control)
                {
                    Shift = ShiftConstants.vbCtrlMask;
                }

                if (e.Alt)
                {
                    Shift = ShiftConstants.vbAltMask;
                }

                if (e.Shift)
                {
                    Shift = ShiftConstants.vbShiftMask;
                }

                if (KeyDownCustom != null)
                {
                    KeyDownCustom.Invoke(KeyCode, Shift);
                }
            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void lstSource_KeyPress(object sender, KeyPressEventArgs e)
        {
            int KeyAscii = KeyAscii = (int)e.KeyChar;
            bool bToSkipSessionTimeOutCheck = false;
            string strParentFormNm = "";

            
            try
            {
                if (this.ParentForm != null)
                {
                    strParentFormNm = this.ParentForm.Name;
                }

                if (CCentralFunction.CCF_HasSessionTimedOut(ref bToSkipSessionTimeOutCheck))
                {
                    return;
                }

                if (KeyPressCustom != null)
                {
                    KeyPressCustom.Invoke(KeyAscii);
                }


            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {
                if (bToSkipSessionTimeOutCheck == false)
                {
                    CCentralFunction.CCF_SaveLastActivityDateTime(this.Name, "", "lstSource_KeyPress", strParentFormNm);
                }
            }
        }

        private void lstSource_KeyUp(object sender, KeyEventArgs e)
        {
            Keys KeyCode = e.KeyCode;
            ShiftConstants Shift = ShiftConstants.vbShiftMask;

            try
            {
                if (e.Control)
                {
                    Shift = ShiftConstants.vbCtrlMask;
                }

                if (e.Alt)
                {
                    Shift = ShiftConstants.vbAltMask;
                }

                if (e.Shift)
                {
                    Shift = ShiftConstants.vbShiftMask;
                }

                if (KeyUpCustom != null)
                {
                    KeyUpCustom.Invoke(KeyCode, Shift);
                }
            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public string List(ListType NewListType, int Index)
        {
            string result = string.Empty;

            try
            {
                if (NewListType == ListType.Source)
                {
                    result = ((ListBoxItem)lstSource.Items[Index]).Item;
                }
                else
                {
                    result = ((ListBoxItem)lstDestination.Items[Index]).Item;
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return result;
        }

        public string List(ListType NewListType, int Index, string New_List)
        {
            try
            {
                if (NewListType == ListType.Source)
                {
                    ((ListBoxItem)lstSource.Items[Index]).Item = New_List;
                }
                else
                {
                    ((ListBoxItem)lstDestination.Items[Index]).Item = New_List;
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return New_List;
        }

        public int ListIndex(ListType NewListType)
        {
            int result = -1;

            try
            {
                if (NewListType == ListType.Destination)
                {
                    result = lstDestination.SelectedIndex;
                }
                else
                {
                    result = lstSource.SelectedIndex;
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return result;
        }

        public int ListCount(ListType NewListType)
        {
            int result = -1;

            try
            {
                if (NewListType == ListType.Destination)
                {
                    result = lstDestination.Items.Count;
                }
                else
                {
                    result = lstSource.Items.Count;
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return result;
        }

        public int ListIndex(ListType NewListType, int New_ListIndex)
        {
            try
            {
                if (NewListType == ListType.Source)
                {
                    if (lstSource.Items.Count > 0)
                    {
                        lstSource.SelectedIndex = New_ListIndex;
                    }
                    else
                    {
                        m_Src_ListIndex = New_ListIndex;
                    }
                    Selected(ListType.Source, New_ListIndex, true);

                }
                else
                {
                    if (lstDestination.Items.Count > 0)
                    {
                        lstDestination.SelectedIndex = New_ListIndex;
                    }
                    else
                    {
                        m_Dest_ListIndex = New_ListIndex;
                    }
                    Selected(ListType.Destination, New_ListIndex, true);
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return New_ListIndex;
        }
        
        private void lstSource_MouseDown(object sender, MouseEventArgs e)
        {
            int button = 0;
            int Shift = 0;
            int X = 0;
            int Y = 0;

            try
            {
                // Get the index of the item the mouse is below.
                indexOfItemUnderMouseToDrag = lstSource.IndexFromPoint(e.X, e.Y);

                if (indexOfItemUnderMouseToDrag != ListBox.NoMatches)
                {
                    // Remember the point where the mouse down occurred. The DragSize indicates
                    // the size that the mouse can move before a drag event should be started.                
                    Size dragSize = SystemInformation.DragSize;

                    // Create a rectangle using the DragSize, with the mouse position being
                    // at the center of the rectangle.
                    dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                                   e.Y - (dragSize.Height / 2)), dragSize);
                }
                else
                {
                    // Reset the rectangle if the mouse is not over an item in the ListBox.
                    dragBoxFromMouseDown = Rectangle.Empty;
                }


                button = (int)e.Button;
                Shift = e.Clicks;
                X = e.X;
                Y = e.Y;

                if (MouseDownCustom != null)
                {
                    MouseDownCustom.Invoke(button, Shift, X, Y);
                }
            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void lstSource_MouseUp(object sender, MouseEventArgs e)
        {
            int button = 0;
            int Shift = 0;
            int X = 0;
            int Y = 0;

            try
            {
                // Reset the drag rectangle when the mouse button is raised.
                dragBoxFromMouseDown = Rectangle.Empty;

                button = (int)e.Button;
                Shift = e.Clicks;
                X = e.X;
                Y = e.Y;

                if (MouseUpCustom != null)
                {
                    MouseUpCustom.Invoke(button, Shift, X, Y);
                }
            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void lstSource_MouseMove(object sender, MouseEventArgs e)
        {
            //VB code needs to be implemented
            //button As Integer, Shift As Integer, X As Single, Y As Single

            //With lstSource    
            //    If button Then  ' Signal a Drag operation.            
            //        ' Set the drag icon
            //        If .SelCount > 1 Then
            //           .DragIcon = imglst.ListImages.Item(2).Picture
            //        Else
            //            .DragIcon = imglst.ListImages(1).Picture
            //        End If            
            //        .Drag vbBeginDrag          
            //    End If        
            //End With
            

            int button = 0;
            int Shift = 0;
            int X = 0;
            int Y = 0;
            try
            {
                if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    // If the mouse moves outside the rectangle, start the drag.
                    if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                    {
                        DragDropEffects dropEffect = lstSource.DoDragDrop(lstSource.Items[indexOfItemUnderMouseToDrag], DragDropEffects.All);
                        cmdMovefields_Click(cmdMovefields0, new EventArgs());
                    }
                }

                button = (int)e.Button;
                Shift = e.Clicks;
                X = e.X;
                Y = e.Y;

                if (MouseMoveCustom != null)
                {
                    MouseMoveCustom.Invoke(button, Shift, X, Y);
                }
            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void lstDestination_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the index of the item the mouse is below.
            indexOfItemUnderMouseToDrag = lstSource.IndexFromPoint(e.X, e.Y);

            if (indexOfItemUnderMouseToDrag != ListBox.NoMatches)
            {
                // Remember the point where the mouse down occurred. The DragSize indicates
                // the size that the mouse can move before a drag event should be started.                
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                               e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
            {
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;
            }
        }

        private void lstDestination_MouseUp(object sender, MouseEventArgs e)
        {
            // Reset the drag rectangle when the mouse button is raised.
            dragBoxFromMouseDown = Rectangle.Empty;
        }

        /*
        Public Property Get MouseIcon() As Picture
            Set MouseIcon = lstSource.MouseIcon
        End Property

        Public Property Set MouseIcon(ByVal New_MouseIcon As Picture)
            Set lstSource.MouseIcon = New_MouseIcon
            PropertyChanged "MouseIcon"
        End Property
        */
        /*
        Public Property Get MousePointer() As Integer
            MousePointer = UserControl.MousePointer
        End Property

        Public Property Let MousePointer(ByVal New_MousePointer As Integer)
            UserControl.MousePointer() = New_MousePointer
            PropertyChanged "MousePointer"
        End Property
        */

        public void RemoveItem(ListType NewListType, int Index)
        {
            try
            {
                if (NewListType == ListType.Source)
                {
                    lstSource.Items.RemoveAt(Index);
                }
                else if (NewListType == ListType.Destination)
                {
                    lstDestination.Items.RemoveAt(Index);
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        //public int ScaleHeight
        //{
        //    get { return this.Height; }
        //    set { this.Height = value; }
        //}

        //public int ScaleLeft
        //{
        //    get { return this.Left; }
        //    set { this.Left = value; }
        //}
        /*
        Public Property Get ScaleMode() As Integer
            ScaleMode = UserControl.ScaleMode
        End Property

        Public Property Let ScaleMode(ByVal New_ScaleMode As Integer)
            UserControl.ScaleMode() = New_ScaleMode
            PropertyChanged "ScaleMode"
        End Property
        */
        //public int ScaleTop
        //{
        //    get { return this.Top; }
        //    set { this.Top = value; }
        //}

        //public int ScaleWidth
        //{
        //    get { return this.Width; }
        //    set { this.Width = value; }
        //}

        public int SelCount(ListType NewListType)
        {
            int result = -1;

            try
            {
                if (NewListType == ListType.Source)
                {
                    result = lstSource.Items.Count;
                }
                else if (NewListType == ListType.Destination)
                {
                    result = lstDestination.Items.Count;
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return result;
        }

        private void UserControl_Initialize()
        {
            m_Src_ListIndex = -1;
            m_Dest_ListIndex = -1;
        }

        private void UserControl_InitProperties()
        {
            this.Height = lstSource.Height + lblCaption0.Height;
            this.Width = lstSource.Width + lstDestination.Width + cmdMovefields0.Width + 30;

            m_Src_QueryString = m_def_QueryString;
            m_Dest_QueryString = m_def_QueryString;

            m_Allowsequencing = true;
            //VB code needs to be implemented
            //lstSource.DragIcon = imglst.ListImages(1).Picture
            //lstDestination.DragIcon = imglst.ListImages(1).Picture    
        }

        /*
        Private Sub UserControl_ReadProperties(PropBag As PropertyBag)
            Dim Index As Integer
            With PropBag    
                lblCaption(0).Caption = .ReadProperty("SetSourceCaption", "")
                lblCaption(1).Caption = .ReadProperty("SetdestinationCaption", "")
                lstSource.Appearance = .ReadProperty("Appearance", 1)
                lstDestination.Appearance = .ReadProperty("Appearance", 1)
                lstSource.BackColor = .ReadProperty("BackColor", &H80000005)
                lstDestination.BackColor = .ReadProperty("BackColor", &H80000005)
                lstSource.ToolTipText = .ReadProperty("ToolTipText", "")
                lstSource.ForeColor = .ReadProperty("ForeColor", &H80000008)
                lstDestination.ForeColor = .ReadProperty("ForeColor", &H80000008)
                If lstSource.ListCount > 0 Then lstSource.ItemData(Index) = .ReadProperty("Src_ItemData" & Index, 0)
                If lstDestination.ListCount > 0 Then lstDestination.ItemData(Index) = .ReadProperty("Dest_ItemData" & Index, 0)
                If lstSource.ListCount > 0 Then lstSource.ListIndex = .ReadProperty("ListIndex", 0)
                If lstDestination.ListCount > 0 Then lstDestination.ListIndex = .ReadProperty("ListIndex", 0)
                Set lstSource.Font = .ReadProperty("Font", Ambient.Font)
                Set lstDestination.Font = .ReadProperty("Font", Ambient.Font)
                Set MouseIcon = .ReadProperty("MouseIcon", Nothing)        
                UserControl.Enabled = .ReadProperty("Enabled", True)
                UserControl.MousePointer = .ReadProperty("MousePointer", 0)
                UserControl.ScaleHeight = .ReadProperty("ScaleHeight", 4890)
                UserControl.ScaleLeft = .ReadProperty("ScaleLeft", 0)
                UserControl.ScaleMode = .ReadProperty("ScaleMode", 1)
                UserControl.ScaleTop = .ReadProperty("ScaleTop", 0)
                UserControl.ScaleWidth = .ReadProperty("ScaleWidth", 4245)
                UserControl.Appearance = .ReadProperty("Appearance", 0)        
                m_Src_SortCol = .ReadProperty("Src_SortColumn", m_def_SortCol)
                m_Dest_SortCol = .ReadProperty("Dest_SortColumn", m_def_SortCol)
                m_Src_SortType = .ReadProperty("Src_SortType", m_def_SortType)
                m_Dest_SortType = .ReadProperty("Dest_SortType", m_def_SortType)
                m_Src_BoundColumn = .ReadProperty("Src_BoundColumn", m_def_BoundColumn)
                m_Dest_BoundColumn = .ReadProperty("Dest_BoundColumn", m_def_BoundColumn)
                Src_QueryString = .ReadProperty("Src_QueryString", "")
                Dest_QueryString = .ReadProperty("Dest_QueryString", "")
                SkipReset = .ReadProperty("SkipReset", False)
                SkipDirty = .ReadProperty("SkipDirty", False)
                AllowSequencing = .ReadProperty("AllowSequencing", False)
                m_Tag = .ReadProperty("Tag", m_def_tag)        
            End With   
        End Sub
        */
        
        private void UserControl_Resize()
        {

            ResizeList();

            if (ResizeCustom != null)
            {
                ResizeCustom.Invoke();
            }

        }
        
        private void PFASAddListControl_Resize(object sender, EventArgs e)
        {
            UserControl_Resize();
        }

        private void UserControl_Show()
        {
            EnableDisableAddlist();
        }
        
        private void PFASAddListControl_Load(object sender, EventArgs e)
        {
            UserControl_Show();
        }

        ///*
        /////Write property values to storage
        //Private Sub UserControl_WriteProperties(PropBag As PropertyBag)
        //    Dim Index As Integer
        //    With PropBag
        //        Call .WriteProperty("Appearance", lstSource.Appearance, 1)
        //        Call .WriteProperty("Appearance", lstDestination.Appearance, 1)
        //        Call .WriteProperty("Enabled", UserControl.Enabled, True)
        //        Call .WriteProperty("Font", lstSource.Font, Ambient.Font)
        //        Call .WriteProperty("Font", lstDestination.Font, Ambient.Font)
        //        Call .WriteProperty("ForeColor", lstSource.ForeColor, &H80000008)
        //        Call .WriteProperty("ForeColor", lstDestination.ForeColor, &H80000008)
        //        If lstSource.ListCount > 0 Then Call .WriteProperty("Src_ItemData" & Index, lstSource.ItemData(Index), 0)
        //        If lstDestination.ListCount > 0 Then Call .WriteProperty("Dest_ItemData" & Index, lstDestination.ItemData(Index), 0)
        //        Call .WriteProperty("BackColor", lstSource.BackColor, &H80000005)
        //        If lstSource.ListCount > 0 Then Call .WriteProperty("List" & Index, lstSource.List(Index), "")
        //        If lstDestination.ListCount > 0 Then Call .WriteProperty("List" & Index, lstDestination.List(Index), "")
        //        If lstSource.ListCount > 0 Then Call .WriteProperty("ListIndex", lstSource.ListIndex, 0)
        //        If lstDestination.ListCount > 0 Then Call .WriteProperty("ListIndex", lstDestination.ListIndex, 0)
        //        Call .WriteProperty("MouseIcon", MouseIcon, Nothing)
        //        Call .WriteProperty("MousePointer", UserControl.MousePointer, 0)
        //        Call .WriteProperty("ScaleHeight", UserControl.ScaleHeight, 4890)
        //        Call .WriteProperty("ScaleLeft", UserControl.ScaleLeft, 0)
        //        Call .WriteProperty("ScaleMode", UserControl.ScaleMode, 1)
        //        Call .WriteProperty("ScaleTop", UserControl.ScaleTop, 0)
        //        Call .WriteProperty("ScaleWidth", UserControl.ScaleWidth, 4245)
        //        Call .WriteProperty("ToolTipText", lstSource.ToolTipText, "")
        //        Call .WriteProperty("Appearance", UserControl.Appearance, 0)
        //        Call .WriteProperty("AllowSequencing", m_Allowsequencing, False)
        //        Call .WriteProperty("SetSourceCaption", lblCaption(0).Caption, "")
        //        Call .WriteProperty("SetDestinationCaption", lblCaption(1).Caption, "")
        //        Call .WriteProperty("Tag", m_Tag, m_def_tag)
        //        Call .WriteProperty("Src_SortColumn", m_Src_SortCol, m_def_SortCol)
        //        Call .WriteProperty("Dest_SortColumn", m_Dest_SortCol, m_def_SortCol)
        //        Call .WriteProperty("Src_SortType", m_Src_SortType, m_def_SortType)
        //        Call .WriteProperty("Dest_SortType", m_Dest_SortType, m_def_SortType)
        //        Call .WriteProperty("SkipReset", m_SkipReset, False)
        //        Call .WriteProperty("SkipDirty", m_SkipDirty, False)
        //        Call .WriteProperty("Src_QueryString", m_Src_QueryString, "")
        //        Call .WriteProperty("Dest_QueryString", m_Dest_QueryString, "")
        //        Call .WriteProperty("Src_BoundColumn", m_Src_BoundColumn, m_def_BoundColumn)
        //        Call .WriteProperty("Dest_BoundColumn", m_Dest_BoundColumn, m_def_BoundColumn)
        //    End With
        //End Sub
        //*/

        public bool SkipReset
        {
            get { return m_SkipReset; }
            set { m_SkipReset = value; }
        }

        public bool SkipDirty
        {
            get { return m_SkipDirty; }
            set { m_SkipDirty = value; }
        }

        [Browsable(false)]
        public bool IsDirty
        {
            get
            {
                if (!m_SkipDirty)
                {
                    return m_IsDirty;
                }
                else
                {
                    //Err.Raise 9999, App.EXEName, "Not Available If SkipDirty Is True. "
                    MessageBox.Show(@"Not Available If SkipDirty Is True.", Path.GetFileName(Assembly.GetExecutingAssembly().Location), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
        }

        public void Requery()
        {
            if (!string.IsNullOrEmpty(m_Src_QueryString))
            {
                Src_PopulateData();
            }
            else if (!string.IsNullOrEmpty(m_Dest_QueryString))
            {
                Dest_PopulateData();
            }
        }

        private void Src_PopulateData()
        {
            CDatabaseClass objCentral = null;
            string retval = string.Empty;
            int I = 0;
            dynamic LocalResult = null;
            int boundcol1 = 0;
            int boundcol2 = 0;

            try
            {
                if (m_Src_QueryString.Length > 0)
                {
                    if (m_Src_BoundColumn.Length > 0)
                    {
                        boundcol1 = Convert.ToInt32(Utility.Left(m_Src_BoundColumn, 1).Trim());
                        boundcol2 = Convert.ToInt32(Utility.Right(m_Src_BoundColumn, 1).Trim());
                    }
                    else
                    {
                        boundcol1 = 1;
                    }

                    if (m_Src_SortCol != "")
                    {
                        if (Utility.InStr(1, Src_QueryString, "order by") == 0)
                        {
                            if (Convert.ToInt32(m_Src_SortType) == 1)
                            {
                                Src_QueryString = Src_QueryString + " order by " + m_Src_SortCol + " asc";
                            }
                            else
                            {
                                Src_QueryString = Src_QueryString + " order by " + m_Src_SortCol + " desc";
                            }
                        }
                        else
                        {
                            if (Utility.InStr(Utility.InStr(1, Src_QueryString, "order by") + 9, Src_QueryString, m_Src_SortCol) == 0)
                            {
                                if (Convert.ToInt32(m_Src_SortType) == 1)
                                {
                                    Src_QueryString = Src_QueryString + " , " + m_Src_SortCol + " asc ";
                                }
                                else
                                {
                                    Src_QueryString = Src_QueryString + " , " + m_Src_SortCol + " desc";
                                }
                            }
                        }
                    }

                    objCentral = new CDatabaseClass();

                    if (objCentral.GetResult(Src_QueryString, ref LocalResult, null, CommandType.Text))
                    {
                        lstSource.Items.Clear();
                        for (I = 0; I <= Information.UBound(LocalResult); I++)
                        {
                            if (m_Src_BoundColumn.Length > 0)
                            {
                                lstSource.Items.Add(new ListBoxItem(LocalResult[I, boundcol1], LocalResult[I, boundcol2]));
                            }
                            else
                            {
                                lstSource.Items.Add(new ListBoxItem(LocalResult[I, boundcol1]));
                            }
                        }

                        if (m_Src_ListIndex != -1)
                        {
                            lstSource.SelectedIndex = m_Src_ListIndex;
                        }
                        else
                        {
                            Selected(ListType.Source, 0, true);
                        }
                    }
                }
                EnableDisableAddlist();
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
            finally
            {
                LocalResult = null;
                objCentral = null;
            }
        }

        private void Dest_PopulateData()
        {
            CDatabaseClass objCentral = null;
            string retval = string.Empty;
            int I = 0;
            dynamic LocalResult = null;
            int boundcol1 = 0;
            int boundcol2 = 0;

            try
            {
                if (m_Dest_QueryString.Length > 0)
                {
                    if (m_Dest_BoundColumn.Length > 0)
                    {
                        boundcol1 = Convert.ToInt32(Utility.Left(m_Dest_BoundColumn, 1).Trim());
                        boundcol2 = Convert.ToInt32(Utility.Right(m_Dest_BoundColumn, 1).Trim());
                    }
                    else
                    {
                        boundcol1 = 0;
                    }

                    if (m_Dest_SortCol != "")
                    {
                        if (Utility.InStr(1, Dest_QueryString, "order by") == 0)
                        {
                            if (Convert.ToInt32(m_Dest_SortType) == 1)
                            {
                                Dest_QueryString = Dest_QueryString + " order by " + m_Dest_SortCol + " asc";
                            }
                            else
                            {
                                Dest_QueryString = Dest_QueryString + " order by " + m_Dest_SortCol + " desc";
                            }
                        }
                        else
                        {
                            if (Utility.InStr(Utility.InStr(1, Dest_QueryString, "order by") + 9, Dest_QueryString, m_Dest_SortCol) == 0)
                            {
                                if (Convert.ToInt32(m_Dest_SortType) == 1)
                                {
                                    Dest_QueryString = Dest_QueryString + " , " + m_Dest_SortCol + " asc ";
                                }
                                else
                                {
                                    Dest_QueryString = Dest_QueryString + " , " + m_Dest_SortCol + " desc";
                                }
                            }
                        }
                    }

                    objCentral = new CDatabaseClass();
                    if (objCentral.GetResult(Dest_QueryString, ref LocalResult, null, CommandType.Text))
                    {
                        lstDestination.Items.Clear();
                        for (I = 0; I <= Information.UBound(LocalResult); I++)
                        {

                            if (m_Dest_BoundColumn.Length > 0)
                            {
                                lstDestination.Items.Add(new ListBoxItem(LocalResult[I, boundcol1], LocalResult[I, boundcol2]));
                            }
                            else
                            {
                                lstDestination.Items.Add(new ListBoxItem(LocalResult[I, boundcol1], LocalResult[I, boundcol2]));
                            }
                        }

                        if (m_Dest_ListIndex != -1)
                        {
                            lstDestination.SelectedIndex = m_Dest_ListIndex;
                        }
                        else
                        {
                            Selected(ListType.Destination, 0, true);
                        }
                    }
                }

                EnableDisableAddlist();
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
            finally
            {
                LocalResult = null;
                objCentral = null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Src_QueryString
        {
            get { return m_Src_QueryString; }
            set
            {
                m_Src_QueryString = value;

                //if (m_Src_QueryString.Trim().Length > 0)
                if (((this.Site == null) || (this.Site.DesignMode == false)) && (m_Src_QueryString.Trim().Length > 0))
                {
                    Src_PopulateData();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Dest_QueryString
        {
            get { return m_Dest_QueryString; }
            set
            {
                m_Dest_QueryString = value;

                //if (m_Dest_QueryString.Trim().Length > 0)
                if (((this.Site == null) || (this.Site.DesignMode == false)) && (m_Dest_QueryString.Trim().Length > 0))
                {
                    Dest_PopulateData();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Src_BoundColumn
        {
            get { return m_Src_BoundColumn; }
            set
            {
                string vNewValue = value;
                if (vNewValue.Trim().Length != 0)
                {
                    if ((vNewValue.Trim().Length != 3) || (!(Information.IsNumeric(Utility.Left(vNewValue, 1))))
                        || (!(Information.IsNumeric(Utility.Right(vNewValue, 1)))) || (Utility.Mid(vNewValue, 2, 1) != ","))
                    {
                        MessageBox.Show(@"Invalid Property Value", Path.GetFileName(Assembly.GetExecutingAssembly().Location), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        m_Src_BoundColumn = Utility.Left(vNewValue, 1).Trim() + "," + Utility.Right(vNewValue, 1);

                        if ((this.Site == null) || (this.Site.DesignMode == false))
                        {
                            Src_PopulateData();
                        }
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Dest_BoundColumn
        {
            get { return m_Dest_BoundColumn; }
            set
            {
                string vNewValue = value;
                if (vNewValue.Trim().Length != 0)
                {
                    if ((vNewValue.Trim().Length != 3) || (!(Information.IsNumeric(Utility.Left(vNewValue, 1)))) || (!(Information.IsNumeric(Utility.Right(vNewValue, 1)))) || (Utility.Mid(vNewValue, 2, 1) != ","))
                    {
                        MessageBox.Show(@"Invalid Property Value", Path.GetFileName(Assembly.GetExecutingAssembly().Location), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        m_Dest_BoundColumn = Utility.Left(vNewValue, 1).Trim() + "," + Utility.Right(vNewValue, 1).Trim();
                        
                        if ((this.Site == null) || (this.Site.DesignMode == false))
                        {
                            Dest_PopulateData();
                        }
                    }
                }
            }
        }

        private void ResizeList()
        {
            try
            {
                this.Height = this.Height < 133 ? 133 : this.Height;
                this.Width = this.Width < 233 ? 233 : this.Width;

                lblCaption0.Location = new System.Drawing.Point(8, 8);

                lstSource.Location = new System.Drawing.Point(8, lblCaption0.Height + 8);
                lstSource.Height = Math.Abs(this.Height - lstSource.Top - 8);
                lstSource.Width = (this.Width - (m_Allowsequencing ? 53 + 41 + 32 : 53 + 24)) / 2;

                cmdMovefields0.Location = new System.Drawing.Point(lstSource.Left + lstSource.Width + 8, lstSource.Top + (lstSource.Height - 97) / 5);
                cmdMovefields0.Width = 53;
                cmdMovefields0.Height = 24;

                cmdMovefields1.Height = cmdMovefields0.Height;
                cmdMovefields1.Width = cmdMovefields0.Width;
                cmdMovefields1.Location = new System.Drawing.Point(cmdMovefields0.Left, cmdMovefields0.Top + cmdMovefields0.Height + (lstSource.Height - 97) / 5);

                cmdMovefields2.Height = cmdMovefields0.Height;
                cmdMovefields2.Width = cmdMovefields0.Width;
                cmdMovefields2.Location = new System.Drawing.Point(cmdMovefields1.Left, cmdMovefields1.Top + cmdMovefields1.Height + (lstSource.Height - 97) / 5);

                cmdMovefields3.Height = cmdMovefields0.Height;
                cmdMovefields3.Width = cmdMovefields0.Width;
                cmdMovefields3.Location = new System.Drawing.Point(cmdMovefields0.Left, cmdMovefields2.Top + cmdMovefields1.Height + (lstSource.Height - 97) / 5);

                lstDestination.Height = lstSource.Height;
                lstDestination.Width = lstSource.Width;
                lstDestination.Location = new System.Drawing.Point(cmdMovefields0.Left + cmdMovefields0.Width + 8, lstSource.Top);

                lblCaption1.Location = new System.Drawing.Point(lstDestination.Left, 8);

                if (m_Allowsequencing)
                {
                    cmdUpDown0.Height = 41;
                    cmdUpDown0.Width = 24;
                    cmdUpDown0.Location = new System.Drawing.Point(lstDestination.Left + lstDestination.Width + 8, lstDestination.Top + (lstDestination.Height / 2) - (8 + cmdUpDown0.Height));

                    cmdUpDown1.Height = cmdUpDown0.Height;
                    cmdUpDown1.Width = cmdUpDown0.Width;
                    cmdUpDown1.Location = new System.Drawing.Point(cmdUpDown0.Left, lstDestination.Top + (lstDestination.Height / 2) + 8);
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        public string Src_SortColumn
        {
            get { return m_Src_SortCol; }
            set { m_Src_SortCol = value; }
        }

        public string Dest_SortColumn
        {
            get { return m_Dest_SortCol; }
            set { m_Dest_SortCol = value; }
        }

        public SortType Src_SortType
        {
            get { return m_Src_SortType; }
            set
            {
                if (Src_QueryString == "")
                {
                    //VB code needs to be implemented
                    //MessageBox.Show(@"Invalid Property Value, Querystring Can not be Blank", Path.GetFileName(Assembly.GetExecutingAssembly().Location), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    m_Src_SortType = value;
                }
            }
        }

        public SortType Dest_SortType
        {
            get { return m_Dest_SortType; }
            set
            {
                if (Dest_QueryString == "")
                {
                    //VB code needs to be implemented
                    //MessageBox.Show(@"Invalid Property Value, Querystring Can not be Blank", Path.GetFileName(Assembly.GetExecutingAssembly().Location), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    m_Dest_SortType = value;
                }
            }
        }

        //VB code needs to be implemented
        /*        
        Public Property Get Appearance() As Appearance
            Appearance = lstSource.Appearance
        End Property

        Public Property Let Appearance(ByVal New_Appearance As Appearance)
            lstSource.Appearance() = New_Appearance
            lstDestination.Appearance() = New_Appearance
            PropertyChanged "Appearance"
        End Property
        */

        public void DoReset()
        {
            int I = 0;

            try
            {
                if ((Src_QueryString == "") && (Dest_QueryString == ""))
                {
                    lstSource.Items.Clear();
                    lstDestination.Items.Clear();

                    for (I = 0; I <= SrcItemCollection.Count - 1; I++)
                    {
                        if (SrcItemDataCollection.Count > 0)
                        {
                            lstSource.Items.Add(new ListBoxItem(SrcItemCollection[I], SrcItemDataCollection[I]));
                        }
                        else
                        {
                            lstSource.Items.Add(new ListBoxItem(SrcItemCollection[I]));
                        }
                    }

                    EnableDisableAddlist();
                    Selected(ListType.Source, lstSource.SelectedIndex, true);

                    m_IsDirty = false;
                }
                else
                {
                    lstSource.Items.Clear();
                    lstDestination.Items.Clear();
                    Requery();
                    m_IsDirty = false;
                }

                if (AfterReset != null)
                {
                    AfterReset.Invoke();
                }
            }
            catch (PFASException pex)
            {
                MessageBox.Show(pex.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
                MessageBox.Show(ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        public void About()
        {
            string strMsg = "Transcom AddList Control." + Environment.NewLine + "Copyright © 2001-2003 Transcom Inc." + Environment.NewLine + "Developed by Transcom Inc., Edison, NJ";
            MessageBox.Show(strMsg, "Transcom Inc.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void Sequencing(int SequenceType)
        {
            //string temp1 = string.Empty;
            //dynamic temp2 = null;
            int iSelectedIndex = -1;

            ///* SequenceType = 1 -- Raise
            ///* SequenceType = -1 -- Lower
            try
            {
                if (GetSelCount(lstDestination) == 0)
                {
                    return;
                }

                ///* Swapping        

                iSelectedIndex = lstDestination.SelectedIndex;

                //temp1 = ((ListBoxItem)lstDestination.Items[iSelectedIndex - (SequenceType)]).Item;
                //temp2 = ((ListBoxItem)lstDestination.Items[iSelectedIndex - (SequenceType)]).Index;

                //((ListBoxItem)lstDestination.Items[iSelectedIndex - (SequenceType)]).Item = ((ListBoxItem)lstDestination.Items[iSelectedIndex]).Item;
                //((ListBoxItem)lstDestination.Items[iSelectedIndex - (SequenceType)]).Index = ((ListBoxItem)lstDestination.Items[iSelectedIndex]).Index;

                //((ListBoxItem)lstDestination.Items[iSelectedIndex]).Item = temp1;
                //((ListBoxItem)lstDestination.Items[iSelectedIndex]).Index = temp2;


                ListBoxItem oListBoxItem1 = (ListBoxItem)lstDestination.Items[iSelectedIndex - (SequenceType)];
                ListBoxItem oListBoxItem2 = (ListBoxItem)lstDestination.Items[iSelectedIndex];

                RemoveItem(ListType.Destination, iSelectedIndex - (SequenceType));
                lstDestination.Items.Insert((iSelectedIndex - (SequenceType)), oListBoxItem2);

                RemoveItem(ListType.Destination, iSelectedIndex);
                lstDestination.Items.Insert(iSelectedIndex, oListBoxItem1);

                Selected(ListType.Destination, iSelectedIndex, false);
                Selected(ListType.Destination, iSelectedIndex - (SequenceType), true);

                if (!m_SkipDirty)
                {
                    m_IsDirty = true;
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        private void EnableDisableAddlist()
        {
            try
            {
                if ((lstSource.Items.Count == 0) && (lstDestination.Items.Count > 0))
                {
                    cmdMovefields0.Enabled = false;
                    cmdMovefields1.Enabled = false;
                    cmdMovefields2.Enabled = true;
                    cmdMovefields3.Enabled = true;
                }
                else if ((lstSource.Items.Count > 0) && (lstDestination.Items.Count > 0))
                {
                    cmdMovefields0.Enabled = true;
                    cmdMovefields1.Enabled = true;
                    cmdMovefields2.Enabled = true;
                    cmdMovefields3.Enabled = true;
                }
                else if ((lstDestination.Items.Count == 0) && (lstSource.Items.Count > 0))
                {
                    cmdMovefields0.Enabled = true;
                    cmdMovefields1.Enabled = true;
                    cmdMovefields2.Enabled = false;
                    cmdMovefields3.Enabled = false;
                }
                else if ((lstSource.Items.Count == 0) && (lstDestination.Items.Count == 0))
                {
                    cmdMovefields0.Enabled = false;
                    cmdMovefields1.Enabled = false;
                    cmdMovefields2.Enabled = false;
                    cmdMovefields3.Enabled = false;
                }

                EnableDisableUpDown();
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        public string Tag
        {
            get { return m_Tag; }
            set { m_Tag = value; }
        }

        public bool AllowSequencing
        {
            get { return m_Allowsequencing; }
            set
            {
                m_Allowsequencing = value;

                if (m_Allowsequencing)
                {
                    cmdUpDown0.Visible = true;
                    cmdUpDown0.Enabled = true;
                    cmdUpDown1.Visible = true;
                    cmdUpDown1.Enabled = true;
                }
                else
                {
                    cmdUpDown0.Visible = false;
                    cmdUpDown0.Enabled = false;
                    cmdUpDown1.Visible = false;
                    cmdUpDown1.Enabled = false;
                }

                ResizeList();
            }
        }

        public string SetSourceCaption
        {
            get { return lblCaption0.Text; }
            set { lblCaption0.Text = value; }
        }

        public string SetDestinationCaption
        {
            get { return lblCaption1.Text; }
            set { lblCaption1.Text = value; }
        }

        ///*It will return the selected(checked) listitems' itemdata
        ///*Itemdatas are the thing we should be interested about??
        public List<dynamic> GetSelectedItems(ListType NewListType)
        {
            int I = 0;
            List<dynamic> SelectedListItemdata = null;

            try
            {
                SelectedListItemdata = new List<dynamic>();

                if (NewListType == ListType.Destination)
                {
                    for (I = 1; I <= lstDestination.Items.Count; I++)
                    {
                        SelectedListItemdata.Add(((ListBoxItem)lstDestination.Items[I - 1]).Index);
                    }
                }
                else
                {
                    for (I = 1; I <= lstSource.Items.Count; I++)
                    {
                        SelectedListItemdata.Add(((ListBoxItem)lstSource.Items[I - 1]).Index);
                    }
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return SelectedListItemdata;
        }

        public List<int> GetSelectedListIndex(ListType NewListType)
        {
            int I = 0;
            List<int> SelectedListIndex = null;

            try
            {
                SelectedListIndex = new List<int>();

                if (NewListType == ListType.Destination)
                {
                    for (I = 1; I <= lstDestination.Items.Count; I++)
                    {
                        if (lstDestination.GetSelected(I - 1) == true)
                        {
                            SelectedListIndex.Add(I - 1);
                        }
                    }
                }
                else
                {
                    for (I = 1; I <= lstSource.Items.Count; I++)
                    {
                        if (lstSource.GetSelected(I - 1) == true)
                        {
                            SelectedListIndex.Add(I - 1);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return SelectedListIndex;
        }

        ///*It will return the selected(checked) listitems' itemdata and the items
        ///*First element of the array will return the listitem and the second element will return the itemdata
        public dynamic GetSelectionList(ListType NewListType, dynamic ItemVal = null, dynamic ItemIndex = null)
        {
            dynamic[,] SelectedItems = null;
            int I = 0;
            int iCount = 0;

            try
            {
                if (NewListType == ListType.Destination)
                {
                    iCount = lstDestination.Items.Count;
                    if (iCount == 0)
                    {
                        return SelectedItems;
                    }

                    SelectedItems = new dynamic[iCount, 2];

                    if ((ItemVal == null) && (ItemIndex == null))
                    {
                        for (I = 0; I <= lstDestination.Items.Count - 1; I++)
                        {
                            SelectedItems[I, 0] = lstDestination.Items[I];
                            SelectedItems[I, 1] = ((ListBoxItem)lstDestination.Items[I]).Index;
                        }
                    }
                    else
                    {
                        if (ItemVal == 0)
                        {
                            SelectedItems[0, 0] = lstDestination.Items[ItemIndex];
                        }
                        else if (ItemVal == 1)
                        {
                            SelectedItems[0, 0] = ((ListBoxItem)lstDestination.Items[ItemIndex]).Index;
                        }
                    }
                }
                else if (NewListType == ListType.Source)
                {
                    iCount = lstSource.Items.Count;
                    if (iCount == 0)
                    {
                        return SelectedItems;
                    }

                    SelectedItems = new dynamic[iCount, 2];

                    if ((ItemVal == null) && (ItemIndex == null))
                    {
                        for (I = 0; I <= lstSource.Items.Count - 1; I++)
                        {
                            SelectedItems[I, 0] = lstSource.Items[I];
                            SelectedItems[I, 1] = ((ListBoxItem)lstSource.Items[I]).Index;
                        }
                    }
                    else
                    {
                        if (ItemVal == 0)
                        {
                            SelectedItems[0, 0] = lstSource.Items[ItemIndex];
                        }
                        else if (ItemVal == 1)
                        {
                            SelectedItems[0, 0] = ((ListBoxItem)lstSource.Items[ItemIndex]).Index;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
            return SelectedItems;
        }

        private int GetSelCount(ListBox listBox)
        {
            int iSelCount = 0;

            try
            {
                for (int i = 0; i <= listBox.Items.Count - 1; i++)
                {
                    if (listBox.GetSelected(i))
                    {
                        iSelCount = iSelCount + 1;
                    }
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }

            return iSelCount;
        }

        private void EnableDisableUpDown()
        {
            try
            {
                if (GetSelCount(lstDestination) != 0)
                {
                    if (lstSource.Items.Count == 0)
                    {
                        cmdUpDown0.Enabled = false;
                        cmdUpDown1.Enabled = true;
                    }
                    else if ((lstSource.Items.Count > 0) && (lstDestination.Items.Count > 0))
                    {
                        if (lstDestination.Items.Count == 1)
                        {
                            cmdUpDown0.Enabled = false;
                            cmdUpDown1.Enabled = false;
                        }
                        else
                        {
                            if (lstDestination.GetSelected(0) == true)
                            {
                                cmdUpDown0.Enabled = false;
                                cmdUpDown1.Enabled = true;
                            }
                            else if (lstDestination.SelectedIndex == lstDestination.Items.Count - 1)
                            {
                                cmdUpDown0.Enabled = true;
                                cmdUpDown1.Enabled = false;
                            }
                            else
                            {
                                cmdUpDown0.Enabled = true;
                                cmdUpDown1.Enabled = true;
                            }
                        }
                    }
                    else if (lstDestination.Items.Count == 1)
                    {
                        cmdUpDown0.Enabled = false;
                        cmdUpDown1.Enabled = true;
                    }
                    else if (lstDestination.Items.Count == 0)
                    {
                        cmdUpDown0.Enabled = false;
                        cmdUpDown1.Enabled = false;
                    }
                }
                else
                {
                    cmdUpDown0.Enabled = false;
                    cmdUpDown1.Enabled = false;
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        public void MoveItem(MoveDirection Direction, bool All = true)
        {
            try
            {
                if (All)
                {
                    if (Direction == MoveDirection.MoveToRight)
                    {
                        cmdMovefields_Click(cmdMovefields1, new EventArgs());
                    }
                    else
                    {
                        cmdMovefields_Click(cmdMovefields3, new EventArgs());
                    }
                }
                else
                {
                    if (Direction == MoveDirection.MoveToRight)
                    {
                        cmdMovefields_Click(cmdMovefields0, new EventArgs());
                    }
                    else
                    {
                        cmdMovefields_Click(cmdMovefields2, new EventArgs());
                    }
                }
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        public void MoveToDestList(List<dynamic> oItmData = null, dynamic ItemDataArray = null)
        {
            int I = 0;
            int J = 0;
            List<int> oIndexToMove = null;

            try
            {
                oIndexToMove = new List<int>();

                for (I = lstSource.Items.Count - 1; I >= 0; I--)
                {
                    Selected(ListType.Source, I, false);
                    if (oItmData != null)
                    {
                        for (J = 0; J <= oItmData.Count - 1; J++)
                        {
                            if (((ListBoxItem)lstSource.Items[I]).Index == oItmData[J])
                            {
                                oIndexToMove.Add(I);
                            }
                        }
                    }

                    if (ItemDataArray != null)
                    {
                        for (J = 0; J <= Information.UBound(ItemDataArray); J++)
                        {
                            if (((ListBoxItem)lstSource.Items[I]).Index == ItemDataArray[J, 0])
                            {
                                oIndexToMove.Add(I);
                                break;
                            }
                        }
                    }
                }

                for (I = 0; I <= oIndexToMove.Count - 1; I++)
                {
                    Selected(ListType.Source, oIndexToMove[I], true);
                }

                MoveItem(MoveDirection.MoveToRight, false);
            }
            catch (Exception Ex)
            {
                Utility.LogError(Ex);
            }
        }

        public void SrcSelected(int Index)
        {
            if (lstSource.Items.Count >= Index)
            {
                for (int lRow = 0; lRow <= lstSource.Items.Count - 1; lRow++)
                {
                    if (lRow != Index)
                    {
                        Selected(ListType.Source, lRow, false);
                    }
                }

                Selected(ListType.Source, Index, true);
            }
        }

        public void Selected(ListType NewListType, int index, bool value)
        {
            if (NewListType == ListType.Source)
            {
                lstSource_Click(lstSource, new EventArgs());
                lstSource.SetSelected(index, value);
            }
            else if (NewListType == ListType.Destination)
            {
                lstDestination_Click(lstDestination, new EventArgs());
                lstDestination.SetSelected(index, value);
            }
        }

        private void lstDestination_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool bToSkipSessionTimeOutCheck = false;

            string strParentFormNm = "";

            try
            {
                if (this.ParentForm != null)
                {
                    strParentFormNm = this.ParentForm.Name;
                }

                if (CCentralFunction.CCF_HasSessionTimedOut(ref bToSkipSessionTimeOutCheck))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
            finally
            {
                if (bToSkipSessionTimeOutCheck == false)
                {
                    CCentralFunction.CCF_SaveLastActivityDateTime(this.Name, "", "lstDestination_KeyPress", strParentFormNm);
                }
            }


        }
    }

    public class ListBoxItem
    {
        public string Item;
        public dynamic Index;

        public ListBoxItem(string _Item)
        {
            Item = _Item;
            Index = 0;
        }

        public ListBoxItem(string _Item, dynamic _Index)
        {
            Item = _Item;
            Index = _Index;

        }

        public override string ToString()
        {
            return this.Item;
        }
    }
}