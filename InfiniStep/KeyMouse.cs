using System;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Gma.System.MouseKeyHook;
using GetWinFormsId;
using System.Windows;
using System.Text;
using System.Data;
using System.Windows.Automation;
using System.Threading;
using OperationInfinity;
using System.Diagnostics;
using System.Dynamic;
using Newtonsoft.Json;
using System.Windows.Interop;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;
using Keyboard = System.Windows.Input.Keyboard;
using Key = System.Windows.Input.Key;
using InfiniStep;

public class KeyMouse
{
    private IKeyboardMouseEvents m_GlobalHook;
    private Form mainWindow;
    DataTable dt;
    private string keybuf;
    private AutomationFocusChangedEventHandler menuFocusEventHandler;
    //private AutomationElement menuBar;
    private string focused;
    private dynamic menuObj;
    public static CancellationTokenSource MouseCancelTS;
    private AutoResetEvent ResetEvent = new AutoResetEvent(false);
    private Stopwatch StopWatch = new Stopwatch();
    //private WindowInteropHelper windowInteropHelper;

    public KeyMouse()
    {
        menuFocusEventHandler = new AutomationFocusChangedEventHandler(OnMenuFocusEvent);
        Automation.AddAutomationFocusChangedEventHandler(menuFocusEventHandler);
    }
    public void Subscribe(Form win)
    {
        m_GlobalHook = Hook.GlobalEvents();
        m_GlobalHook.MouseClick += GlobalHookMouseClick;
        m_GlobalHook.KeyDown += GlobalHookKeyDown;
        m_GlobalHook.KeyUp += GlobalHookKeyUP;
        ResetEvent.Set();
        keybuf = string.Empty;
        PressShift = false;
        PressCtrl = false;
        PressAlt = false;
        mainWindow = win;
        //dt = win.dt;
        //windowInteropHelper = new WindowInteropHelper(win);
        Processes.Clear();
    }
    public void Unsubscribe()
    {
        m_GlobalHook.MouseClick -= GlobalHookMouseClick;
        m_GlobalHook.KeyDown -= GlobalHookKeyDown;
        m_GlobalHook.KeyUp -= GlobalHookKeyUP;

        m_GlobalHook.Dispose();
        ResetEvent.Set();
        PressShift = false;
        PressCtrl = false;
        PressAlt = false;
    }

    static bool PressShift;
    static bool PressCtrl;
    static bool PressAlt;
    static bool PressWin;


    public static KeysConverter kc = new KeysConverter();
    static Dictionary<string,HashSet<int>> Processes = new Dictionary<string,HashSet<int>>();

    async private void GlobalHookKeyDown(object sender, KeyEventArgs e)
    {
        await Task.Run(() => ResetEvent.WaitOne());
        try
        {

            Debug.WriteLine("KeyDown: \t\t{0}", e.KeyData);
            if (((int)e.KeyCode).isBetween(48,90))
            {
                string key = kc.ConvertToString(e.KeyCode);
                if (e.Shift ^ !Keyboard.IsKeyToggled(Key.CapsLock))
                    key = key.ToLower();

                //addItem("KeyInput", key);
                keybuf += key;
            }
            else
            {
                CommitBuf();
                //if(!string.IsNullOrEmpty(keybuf))
                //{
                //    addItem("KeyInput", keybuf );
                //    keybuf = string.Empty;
                //}
                if (e.KeyCode.In(Keys.LShiftKey, Keys.RShiftKey))
                {
                    if (PressShift)
                        return;
                    else
                        PressShift = true;

                }
                if (e.KeyCode.In(Keys.RControlKey, Keys.LControlKey))
                {
                    if (PressCtrl)
                        return;
                    else
                        PressCtrl = true;
                }
                if (e.KeyCode.In(Keys.LMenu, Keys.RMenu))
                {
                    if (PressAlt)
                        return;
                    else
                        PressAlt = true;
                }
                else if (e.KeyCode == Keys.LWin)
                {
                    if (PressWin)
                        return;
                    else
                        PressWin= true;
                }
                else if (e.KeyCode == Keys.RWin)
                {//!
                    if (PressWin)
                        return;
                    else
                        PressWin = true;

                }
                addItem("SpecialKeysDown", new { KeyCode = e.KeyData, Name = e.KeyData.ToString() });
            }
            Console.WriteLine("KeyDown: \t\t{0}", e.KeyCode);
        }
        finally
        {
            //mainWindow.ScrollBottomGrid();
            ResetEvent.Set();
        }
    }
    async private void GlobalHookKeyUP(object sender, KeyEventArgs e)
    {
        await Task.Run(() => ResetEvent.WaitOne(10,true));
        Console.WriteLine("GlobalHookKeyUP: \t\t{0}", e.KeyCode);
        if (e.KeyCode.In(Keys.LShiftKey,Keys.RShiftKey ,Keys.RControlKey,Keys.LControlKey,
                          Keys.LMenu,Keys.RMenu,Keys.LWin,Keys.RWin))
        {
            CommitBuf();
            addItem("SpecialKeysUp", new { e.KeyCode , Name = e.KeyCode.ToString()});
        }
        if (e.KeyCode.In(Keys.LShiftKey,Keys.RShiftKey))
        {
            PressShift = false;
        }
        if (e.KeyCode.In(Keys.RControlKey,Keys.LControlKey))
        {
            PressCtrl = false;
        }
        if (e.KeyCode.In(Keys.LMenu,Keys.RMenu))
        {
            PressAlt = false;
        }
        if (e.KeyCode.In(Keys.LWin, Keys.RWin))
        {
            PressWin = false;
        }


        ResetEvent.Set();

    }
    void CommitBuf()
    {
        if (!string.IsNullOrEmpty(keybuf))
        {
            addItem("KeyInput", keybuf);
            keybuf = string.Empty;
        }
    }
    System.Drawing.Point lastPoint;

    async private void GlobalHookMouseClick(object sender, MouseEventArgs e)
    {
        var elapsed = StopWatch.ElapsedMilliseconds;
        CommitBuf();

        var mainWndHandle = mainWindow.Handle;//windowInteropHelper.Handle;
        var point = new System.Drawing.Point(e.X, e.Y);
        WinFormsUtilities.NativeMethods.GetPhysicalCursorPos(ref point);

        IntPtr wnd = WinFormsUtilities.NativeMethods.WindowFromPoint(point.X, point.Y);
        //wnd = WinFormsUtilities.GetTopmostHwnd(wnd);
        if (wnd == IntPtr.Zero)
            return;

        if (wnd == mainWndHandle)
            return;

        AutomationElement element = null;
        try
        {
            element = AutomationElement.FromHandle(wnd);
        }
        catch
        {
            element = AutomationElement.FromPoint(new Point(point.X, point.Y));
        }
        if (element == null || !element.Current.IsControlElement)
            return;

        StopWatch.Restart();
        bool DoubleClick=false;
        if (IsSamePoint(point,lastPoint) && elapsed <= SystemInformation.DoubleClickTime + 200)
        {
            MouseCancelTS.Cancel();

            //var lastRow = dt.Rows.Cast<DataRow>().Last();
            //var cells = lastRow.ItemArray;
            //var cmd = JsonConvert.DeserializeObject<Dictionary<string, ExpandoObject>>("{" + cells.First().ToString() +"}" );
            //string json = JsonConvert.SerializeObject(new Dictionary<string, ExpandoObject> { { "MouseDoubleClick", cmd.Values.First() } });
            //json =json.Substring(1, json.Length - 2);
            //lastRow.ItemArray = new []{json};
            //return;
            DoubleClick = true;
            await Task.Run(() => ResetEvent.WaitOne(5, true));
        }

        //await Task.Run(() => ResetEvent.WaitOne(5,true));

        dynamic itemParams = new ExpandoObject();
        AutomationElement topElement = null;
        try
        {
            lastPoint = point;
            if (!string.IsNullOrEmpty(element.Current.Name))
                itemParams.Name = element.Current.Name;

            if (!string.IsNullOrEmpty(element.Current.AutomationId) && element.Current.Name != element.Current.AutomationId)
                itemParams.Control = element.Current.AutomationId;

            topElement = AutomationElement.FromHandle(WinFormsUtilities.GetTopWindow(wnd));
            if (topElement != null)
            {
                if (!string.IsNullOrEmpty(topElement.Current.AutomationId))
                    itemParams.Window = topElement.Current.AutomationId;
                itemParams.WindowTitle = topElement.Current.Name;
            }
            var p = Process.GetProcessById(element.Current.ProcessId);
            Processes.AddValue(p.ProcessName, p.Id);
            itemParams.ProcessName = p.ProcessName;
            var pIndex = Processes[p.ProcessName].ToList().IndexOf(p.Id);
            if (pIndex > 0)
            {
                itemParams.ProcessNo = pIndex;
            }
            itemParams.Framework = element.Current.FrameworkId;

            MouseCancelTS = new CancellationTokenSource();
            await Task.Run(() =>
            {
                using (MouseCancelTS.Token.Register(Thread.CurrentThread.Abort))
                    MouseClickThread(e, element, itemParams, topElement, point, DoubleClick);
            }, MouseCancelTS.Token);
        }
        catch (ElementNotAvailableException)
        {
            Debug.WriteLine("ElementNotAvailableException caused");
            var operation = "MouseClick";
            if (e.Button == MouseButtons.Right)
                operation = "MouseRightClick";

            if (DoubleClick)
                operation = "MouseDoubleClick";

            if (itemParams != null)
                addItem(operation, itemParams);
        }
        finally
        {
            ResetEvent.Set();
        }
        //mainWindow.ScrollBottomGrid();
    }
    private bool IsSamePoint(System.Drawing.Point p1, System.Drawing.Point p2)
    {
        return Math.Abs(p1.X - p2.X) < 10 && Math.Abs(p1.Y - p2.Y) <10 ;
    }
    private void MouseClickThread(MouseEventArgs e,  AutomationElement element, dynamic itemParams, AutomationElement topElement, System.Drawing.Point point, bool DoubleClick)
    {

        var operation = "MouseClick";
        if (e.Button == MouseButtons.Right)
            operation = "MouseRightClick";

        if (DoubleClick)
            operation = "MouseDoubleClick";

        try
        {
            if (element.Current.ControlType.In(ControlType.Pane))
                //if (element.Current.FrameworkId == "WinForm" && element.Current.ControlType.In(ControlType.Pane))
            {
                element = AutomationElement.FromHandle((IntPtr)element.Current.NativeWindowHandle);
            }
            if (!((ExpandoObject)itemParams).Exsits("Control") && !((ExpandoObject)itemParams).Exsits("Name"))
                if (string.IsNullOrEmpty(element.Current.AutomationId))
                    itemParams.Control = element.Current.Name;
                else
                    itemParams.Control = element.Current.AutomationId;


            var ctlType = element.Current.ControlType.ProgrammaticName.ToString();
            itemParams.ControlType = ctlType.Substring("ControlType.".Length);

            if (topElement.Current.ClassName != "#32770")
            {
                int iaid;
                if (int.TryParse(element.Current.AutomationId, out iaid))
                {
                    //for ComboBox TextArea
                    if (iaid > 0)
                        element = AutomationElement.FromPoint(element.Current.BoundingRectangle.BottomRight);
                    //itemParams.Control = element.Current.AutomationId;
                    if(element.Current.AutomationId == iaid.ToString())
                        itemParams.Framework = element.Current.FrameworkId;
                }
            }
            if (element.Current.FrameworkId.In("Win32", "DirectUI"))
            {
                itemParams.W32ClassName = topElement.Current.ClassName;
                if (element.Current.ClassName == "TaskListThumbnailWnd")
                {
                    //!
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    IntPtr wnd = IntPtr.Zero;
                    do
                    {
                        Thread.Sleep(50);
                        //var p = AutomationElement.FocusedElement.Current.BoundingRectangle.TopLeft;
                        wnd = WinFormsUtilities.NativeMethods.GetForegroundWindow();
                        //WinFormsUtilities.NativeMethods.GetWindowThreadProcessId(wnd,out prc);
                        
                        //wnd = WinFormsUtilities.NativeMethods.WindowFromPoint((int)p.X, (int)p.Y);
                    } while (wnd == IntPtr.Zero && wnd != (IntPtr)element.Current.NativeWindowHandle && sw.ElapsedMilliseconds < 3000);
                    var target = AutomationElement.FromHandle(wnd);
                    //var p = AutomationElement.FocusedElement.Current.BoundingRectangle.TopLeft;
                    //target = AutomationElement.FromPoint(p);
                    var proc = Process.GetProcessById(target.Current.ProcessId);
                    if (!string.IsNullOrEmpty(target.Current.AutomationId))
                        itemParams.Window = target.Current.AutomationId;
                    if (!string.IsNullOrEmpty(proc.MainWindowTitle))
                        itemParams.WindowTitle = proc.MainWindowTitle;
                    else
                        itemParams.WindowTitle = target.Current.Name;
                    itemParams.ProcessName = proc.ProcessName;
                    ((IDictionary<string, Object>)itemParams).Remove("Name");
                    ((IDictionary<string, Object>)itemParams).Remove("Control");
                    //itemParams.Name = null;
                    //itemParams.Control = null;
                }

                if (itemParams.W32ClassName =="ComboLBox" && element.Current.ControlType.In(ControlType.List))
                {
                    //for combobox　▼
                    string id = element.Current.AutomationId;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (!string.IsNullOrEmpty(id) && element.Current.AutomationId == id && sw.ElapsedMilliseconds < 3000)
                    {
                        Thread.Sleep(200);
                        AutomationElement parent = AutomationElement.FocusedElement;
                        if (parent.Current.ControlType != ControlType.ComboBox) parent = AutomationElement.FromPoint(parent.Current.BoundingRectangle.TopRight);
                        itemParams.Control = parent.Current.AutomationId;
                        IntPtr wnd = WinFormsUtilities.NativeMethods.WindowFromPoint(point.X, point.Y);
                        //wnd = (IntPtr)parent.Current.NativeWindowHandle;
                        topElement = AutomationElement.FromHandle(WinFormsUtilities.GetTopWindow(wnd));
                        if (!string.IsNullOrEmpty(topElement.Current.AutomationId))
                            itemParams.Window = topElement.Current.AutomationId;
                        SelectionPattern selectionPattern = parent.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
                        var sel = selectionPattern.Current.GetSelection();
                        if (sel.Length > 0)
                        {
                            itemParams.SubItem = selectionPattern.Current.GetSelection()[0].Current.Name;
                            break;
                        }

                    }
                    return;
                }
                else if (element.Current.ControlType.In(ControlType.List, ControlType.Tree))
                {
                    SetSelectedItem(itemParams, element,point);

                    itemParams.ClassName = element.Current.ClassName;
                    return;
                }

                else
                {
                    itemParams.ClassName = element.Current.ClassName;
                    //var topLeft = element.Current.BoundingRectangle.TopLeft;
                    //itemParams.Point = new Point(point.X - topLeft.X, point.Y - topLeft.Y);
                }

            }
            if (element.Current.ControlType.In(ControlType.Edit,ControlType.Document))
            {
                var tp = element.GetCurrentPattern(TextPattern.Pattern) as TextPattern;
                if (tp.SupportedTextSelection != SupportedTextSelection.None)
                {                  
                    var trs = tp.GetSelection();
                    var r =trs.First().GetBoundingRectangles();
                    if (r.Length ==0)
                    {
                        SendKeys.SendWait("^+{HOME}");
                        trs = tp.GetSelection();
                        var selected = trs.First().GetText(-1).Replace("\n", "");
                        itemParams.Pos = selected.Length;
                        if (selected.Contains("\r"))
                            SendKeys.SendWait("{LEFT}");
                        
                        SendKeys.SendWait("{RIGHT}".Repeat(selected.Length));
                    }
                    else
                    {
                        //itemParams.Rect = r.First();
                        var selected = trs.First().GetText(-1).Replace("\n","");
                        itemParams.Selected = selected;
                        //itemParams.Pos = element.Current.Name.IndexOf(selected);
                        SendKeys.SendWait("^+{HOME}");
                        trs = tp.GetSelection();
                        var newSelected = trs.First().GetText(-1).Replace("\n", "");
                        itemParams.Pos = newSelected.Length;

                        if (newSelected.Contains("\r"))
                        {
                            SendKeys.SendWait("{Left}");
                        }
                        
                        if (newSelected.EndsWith(selected))
                        {
                            if (newSelected != selected)
                                SendKeys.SendWait("{RIGHT}".Repeat(newSelected.Length - selected.Length));
                            else
                                return;
                        }
                        else
                            SendKeys.SendWait("{RIGHT}".Repeat(newSelected.Length));

                        SendKeys.SendWait("+{RIGHT}".Repeat(selected.Length));
                    }
                }
            }
            else if (element.Current.ControlType.In(ControlType.Tree, ControlType.DataGrid, ControlType.List, ControlType.Tab))
            {
                //for list
                SetSelectedItem(itemParams, element,point);
            }
            else if (element.Current.ControlType == ControlType.Table)
            {
                //DataGridView
                var cells = element.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));
                if (cells.Count > 0)
                {
                    foreach (AutomationElement cellEle in cells)
                    {
                        var rect = cellEle.Current.BoundingRectangle;
                        if (rect.Contains(e.X, e.Y) && cellEle.Current.AutomationId != element.Current.AutomationId && cellEle.Current.IsEnabled)
                        {
                            string pos = cellEle.Current.Name;
                            var sp = pos.Split(' ');
                            itemParams.Row = sp.Last();
                            if (sp.Length < 3)
                                continue;

                            itemParams.Column = sp[0];
                            break;
                        }
                    }
                }
            }
            else if (element.Current.ControlType == ControlType.Menu)
            {
                //menu items
                if (menuObj != null)
                {
                    itemParams.Control = menuObj.MenuStripId;
                    itemParams.Window = menuObj.Window;
                    itemParams.Menu = menuTop;
                    itemParams.MenuItem = focused;
                    menuObj = null;
                }
                else
                {
                    itemParams.ContextMenuItem = focused;
                }
            }
            else if (element.Current.ControlType.In(ControlType.ToolBar, ControlType.MenuBar))
            {
                AutomationElementCollection subelements;
                AutomationElement subelement;
                if (element.Current.ControlType.In(ControlType.MenuBar))
                {
                    var selMenu = topElement.FindFirst(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
                    if (selMenu != null && selMenu.Current.ControlType == ControlType.Menu)
                    {
                        menuObj = new ExpandoObject();
                        menuObj.Window = itemParams.Window;
                        menuObj.MenuStripId = element.Current.AutomationId;
                        itemParams.Menu = selMenu.Current.Name;
                        return;
                    }
                    //menubar root item
                    subelements = topElement.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuBar));
                    subelement = subelements.Cast<AutomationElement>().FirstOrDefault(x => x.Current.Name == element.Current.Name);
                }
                else
                {
                    //toolstrip
                    subelements = topElement.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
                    subelement = subelements.Cast<AutomationElement>().FirstOrDefault(x => x.Current.Name == element.Current.Name);
                }
                if (subelement != null)
                {
                    foreach (AutomationElement it in subelement.FindInRawView())
                    {
                        var rect = it.Current.BoundingRectangle;
                        if (!string.IsNullOrEmpty(it.Current.AutomationId))
                        {
                            if(it.Current.AutomationId == element.Current.AutomationId)
                                continue;
                        }
                        else if (!string.IsNullOrEmpty(it.Current.Name) && it.Current.Name == element.Current.Name)
                            continue;

                        if (rect.Contains(e.X, e.Y) && it.Current.IsEnabled)
                        {
                            itemParams.SubItem = it.Current.Name;
                            break;
                        }
                    }
                }
            }
            else 
            {
                Point pt;
                if (element.Current.ControlType.In(ControlType.Window,ControlType.Pane,ControlType.Group,ControlType.Image,ControlType.Custom,ControlType.Document,
                                                   ControlType.ScrollBar,ControlType.Slider,ControlType.Spinner,ControlType.Calendar,ControlType.DataGrid,ControlType.StatusBar)
                                                   || e.Button == MouseButtons.Right)
                {
                    var topLeft = element.Current.BoundingRectangle.TopLeft;
                    itemParams.Point = new Point(point.X - topLeft.X, point.Y - topLeft.Y);
                }
            }
            
        }
        //catch (ElementNotAvailableException)
        //{
        //    System.Diagnostics.Debug.WriteLine("ElementNotAvailableException caused");
        //}
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }
        finally
        {
            if (itemParams!=null)
                addItem(operation, itemParams);
            keybuf = string.Empty;
            //mainWindow.Dispatcher.Invoke((Action)(() =>
            //{
            //    mainWindow.ScrollBottomGrid();
            //}));
            ResetEvent.Set();
        }
    }
    //string[] uncommited;
    string menuTop;
    ///string menuId;
    private void OnMenuFocusEvent(object src, AutomationFocusChangedEventArgs e)
    {
        var element = src as AutomationElement;
        try
        {
            if (element.Current.ControlType.In(ControlType.ComboBox , ControlType.MenuItem))
            {
                // System.Diagnostics.Debug.WriteLine(element.Current.Name + "/" + element.Current.AutomationId);
                if (element.FindFirst(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition) != null)//(element.Current.Name.EndsWith("DropDown") && element.Current.Name.Length > "DropDown".Length)
                    menuTop = element.Current.Name;
                else
                    focused = element.Current.Name;
            }

            else
            {
                menuTop = "";
                focused = "";
            }
        }
        catch (ElementNotAvailableException )
        {
            System.Diagnostics.Debug.WriteLine("ElementNotAvailableException caused");
        }
     }
    //private void OnUIAutomationEvent(object src, AutomationEventArgs e)
    //{
    //    var element = src as AutomationElement;
    //    addItem("MouseClick ", element.Current.AutomationId, "", element.Current.Name);
    //}
    //private void Property_Change_Event(object src, AutomationPropertyChangedEventArgs e)
    //{
    //    AutomationElement element = src as AutomationElement;
    //    //if (e.Property.ProgrammaticName == "AA")
    //    {
    //        //var topWnd = AutomationElement.FromHandle(WinFormsUtilities.GetTopWindow((IntPtr)element.Current.NativeWindowHandle));
    //        foreach( var wnd in  procClosedHandler.Where(x => AutomationElement.FromHandle(x.Key) == null).Select(x=>x.Key))
    //        {
    //            Automation.RemoveAutomationFocusChangedEventHandler(procFocusedHandler[wnd]);
    //            procFocusedHandler.Remove(wnd);
    //            Automation.RemoveAutomationPropertyChangedEventHandler(null,procClosedHandler[wnd]);
    //            procClosedHandler.Remove(wnd);
    //        }

    //    }
    //}


    void addItem(string str)
    {
        dt.Rows.Add(new string[] { str });
    }

    void addItem(string Operation, dynamic obj)
    {
//        mainWindow.Dispatcher.Invoke((Action)(() =>
//        {
//            string json = JsonConvert.SerializeObject(new Dictionary<string,object>{{ Operation , obj }});
////            dt.Rows.Add(new string[] { "\"" + Operation + "\" : " + json });
//            dt.Rows.Add(json.Substring(1,json.Length-2));
//        }));
    }



    void SetSelectedItem(dynamic itemParams, AutomationElement element,System.Drawing.Point point)
    {
        //Thread.Sleep(200);
        SelectionPattern selectionPattern = element.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
        var sel = selectionPattern.Current.GetSelection();
        if (sel.Length == 0 || !sel[0].Current.BoundingRectangle.Contains(point.X, point.Y))
        {
            var topLeft = element.Current.BoundingRectangle.TopLeft;
            itemParams.Point = new Point(point.X - topLeft.X, point.Y - topLeft.Y);
            return;
        }

        itemParams.SubItem = sel[0].Current.Name;



        //treeview
        //var sub = selectionPattern.Current.GetSelection()[0];
        if (element.Current.ControlType == ControlType.Tree)
        {
            //AutomationPattern automationPatternFromElement = play.GetSpecifiedPattern(sel[0], "ExpandCollapsePatternIdentifiers.Pattern");
            //ExpandCollapsePattern expandCollapsePattern = sel[0].GetCurrentPattern(automationPatternFromElement) as ExpandCollapsePattern;
            //if (expandCollapsePattern.Current.ExpandCollapseState != ExpandCollapseState.LeafNode)
            //    itemParams.Expand = expandCollapsePattern.Current.ExpandCollapseState == ExpandCollapseState.Expanded;

        }
    }
}
