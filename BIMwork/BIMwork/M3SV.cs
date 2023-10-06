using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;


namespace BIMwork
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class M3SV : IExternalApplication
    {
        private const string TAB_NAME = "BIMWork";

        private const string PANEL_LEADER_NAME = "アングル";
        private const string GROUP_LEADER_NAME = "引出線";

        private const string PANEL_CONNECTION_NAME = "許可・禁⽌";
        private const string GROUP_CONNECTION_NAME = "端点⼀括結合許可・禁⽌";

        private const string PANEL_ALIGNMENT_NAME = "ソート";
        private const string GROUP_ALIGNMENT_NAME = "整列";

        private const string PANEL_CAD_RELOAD_NAME = "Reload";
        private const string BTN_CAD_RELOAD_NAME = "CAD 再ロード";

        private const string PANEL_DELETE_DEFAULT_TYPE_NAME = "Delete";
        private const string BTN_DELETE_DEFAULT_NAME = "デフォルトタイプ削除";

        private const string PANEL_BOUNDING_BOX_ALIGNMENT_DEFAULT_TYPE_NAME = "Alignment";
        private const string BTN_BOUNDING_BOX_ALIGNMENT_DEFAULT_NAME = "Bounding Box Alignment";

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // create tab
                application.CreateRibbonTab(TAB_NAME);

                // leader
                RibbonPanel leaderPanel = application.CreateRibbonPanel(TAB_NAME, PANEL_LEADER_NAME);
                createLeaderPanel(ref leaderPanel);

                // connection
                RibbonPanel connectionPanel = application.CreateRibbonPanel(TAB_NAME, PANEL_CONNECTION_NAME);
                createConnectionEndpointPanel(ref connectionPanel);

                // alignment
                RibbonPanel alignmentPanel = application.CreateRibbonPanel(TAB_NAME, PANEL_ALIGNMENT_NAME);
                createAlignmentPanel(ref alignmentPanel);

                // CAD reload
                RibbonPanel cadReloadPanel = application.CreateRibbonPanel(TAB_NAME, PANEL_CAD_RELOAD_NAME);
                createCADReload(ref cadReloadPanel);

                // delete default type
                RibbonPanel deleteDefaultPanel = application.CreateRibbonPanel(TAB_NAME, PANEL_DELETE_DEFAULT_TYPE_NAME);
                createDeleteDefaultType(ref deleteDefaultPanel);

                //
                RibbonPanel boundingBoxAlignment = application.CreateRibbonPanel(TAB_NAME, PANEL_BOUNDING_BOX_ALIGNMENT_DEFAULT_TYPE_NAME);
                createBoundingBoxAligment(ref boundingBoxAlignment);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                return Result.Failed;
            }
        }


        private string getApplicationResourcesPath(string icon)
        {
            string smAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string smAppResourcePath = smAssemblyPath + "\\Resources\\" + icon;
            return smAppResourcePath;
        }

        private void createLeaderPanel(ref RibbonPanel panel)
        {
            // Get Assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyFullName = assembly.Location;

            PulldownButtonData leaderGroupData = new PulldownButtonData("PULLDOWN_GROUP_LEADER", GROUP_LEADER_NAME);
            PulldownButton leaderGroup = panel.AddItem(leaderGroupData) as PulldownButton;
            leaderGroup.ToolTip = "Converts the leader lines 60/90/120-degrees angle leader lines";
            // group1.LongDescription = "<p>Hello,</p><p>I am ComboBox #1.</p><p>Regards,</p>";

            Uri uri16x16 = new Uri(getApplicationResourcesPath("angle_16x16.png"));
            Uri uri32x32 = new Uri(getApplicationResourcesPath("angle_32x32.png"));
            leaderGroup.Image = new BitmapImage(uri16x16);
            leaderGroup.LargeImage = new BitmapImage(uri32x32);

            // leader 60
            PushButtonData leader60ItemData = new PushButtonData("BTN_LEADER_60", "引出線 60°",
                assemblyFullName, "BIMwork.Angle60");
            PushButton leader60Item = leaderGroup.AddPushButton(leader60ItemData);
            leader60Item.ToolTip = "Converts the leader lines 60-degrees angle leader lines";
            Uri leader60Uri16x16 = new Uri(getApplicationResourcesPath("angle_60_16x16.png"));
            Uri leader60Uri32x32 = new Uri(getApplicationResourcesPath("angle_60_32x32.png"));
            leader60Item.Image = new BitmapImage(leader60Uri16x16);
            leader60Item.LargeImage = new BitmapImage(leader60Uri32x32);

            // leader 90
            PushButtonData leader90ItemData = new PushButtonData("BTN_LEADER_90", "引出線 90°",
                assemblyFullName, "BIMwork.Angle90");
            PushButton leader90Item = leaderGroup.AddPushButton(leader90ItemData);
            leader90Item.ToolTip = "Converts the leader lines 90-degrees angle leader lines";
            Uri leader90Uri16x16 = new Uri(getApplicationResourcesPath("angle_90_16x16.png"));
            Uri leader90Uri32x32 = new Uri(getApplicationResourcesPath("angle_90_32x32.png"));
            leader90Item.Image = new BitmapImage(leader90Uri16x16);
            leader90Item.LargeImage = new BitmapImage(leader90Uri32x32);

            // leader 120
            PushButtonData leader12ItemData = new PushButtonData("BTN_LEADER_120", "引出線 120°",
                assemblyFullName, "BIMwork.Angle120");
            PushButton leader120Item = leaderGroup.AddPushButton(leader12ItemData);
            leader120Item.ToolTip = "Converts the leader lines 120-degrees angle leader lines";
            Uri leader120Uri16x16 = new Uri(getApplicationResourcesPath("angle_120_16x16.png"));
            Uri leader120Uri32x32 = new Uri(getApplicationResourcesPath("angle_120_32x32.png"));
            leader120Item.Image = new BitmapImage(leader120Uri16x16);
            leader120Item.LargeImage = new BitmapImage(leader120Uri32x32);

            leaderGroup.AddSeparator();
        }

        private void createAlignmentPanel(ref RibbonPanel panel)
        {
            // Get Assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyFullName = assembly.Location;

            PulldownButtonData alignmentGroupData = new PulldownButtonData("PULLDOWN_GROUP_ALIGNMENT", GROUP_ALIGNMENT_NAME);
            PulldownButton alignmentGroup = panel.AddItem(alignmentGroupData) as PulldownButton;
            alignmentGroup.ToolTip = "整列";
            // group1.LongDescription = "<p>Hello,</p><p>I am ComboBox #1.</p><p>Regards,</p>";

            Uri uri16x16 = new Uri(getApplicationResourcesPath("alignment_16x16.png"));
            Uri uri32x32 = new Uri(getApplicationResourcesPath("alignment_32x32.png"));
            alignmentGroup.Image = new BitmapImage(uri16x16);
            alignmentGroup.LargeImage = new BitmapImage(uri32x32);

            // X-寄せ【左】
            PushButtonData xLeftAlignItemData = new PushButtonData("BTN_X_LEFT_ALIGN", "X-寄せ【左】",
                assemblyFullName, "BIMwork.XLeftAlign");
            PushButton xLeftAlignItem = alignmentGroup.AddPushButton(xLeftAlignItemData);
            xLeftAlignItem.ToolTip = "X-寄せ【左】";
            Uri xLeftAlignUri16x16 = new Uri(getApplicationResourcesPath("left_alignment_16x16.png"));
            Uri xLeftAlignUri32x32 = new Uri(getApplicationResourcesPath("left_alignment_32x32.png"));
            xLeftAlignItem.Image = new BitmapImage(xLeftAlignUri16x16);
            xLeftAlignItem.LargeImage = new BitmapImage(xLeftAlignUri32x32);

            // X-寄せ【右】
            PushButtonData xRightAlignItemData = new PushButtonData("BTN_X_RIGHT_ALIGN", "X-寄せ【右】",
                assemblyFullName, "BIMwork.XRightAlign");
            PushButton xRightAlignItem = alignmentGroup.AddPushButton(xRightAlignItemData);
            xLeftAlignItem.ToolTip = "X-寄せ【右】";
            Uri xRightAlignUri16x16 = new Uri(getApplicationResourcesPath("right_alignment_16x16.png"));
            Uri xRightAlignUri32x32 = new Uri(getApplicationResourcesPath("right_alignment_32x32.png"));
            xRightAlignItem.Image = new BitmapImage(xRightAlignUri16x16);
            xRightAlignItem.LargeImage = new BitmapImage(xRightAlignUri32x32);

            // X-中央寄せ
            PushButtonData xCenterAlignItemData = new PushButtonData("BTN_X_CENTER_ALIGN", "X-中央寄せ",
                assemblyFullName, "BIMwork.XCenterAlign");
            PushButton xCenterAlignItem = alignmentGroup.AddPushButton(xCenterAlignItemData);
            xLeftAlignItem.ToolTip = "X-中央寄せ";
            Uri xCenterAlignUri16x16 = new Uri(getApplicationResourcesPath("x_center_alignment_16x16.png"));
            Uri xCenterAlignUri32x32 = new Uri(getApplicationResourcesPath("x_center_alignment_32x32.png"));
            xCenterAlignItem.Image = new BitmapImage(xCenterAlignUri16x16);
            xCenterAlignItem.LargeImage = new BitmapImage(xCenterAlignUri32x32);

            // Y-中央寄せ
            PushButtonData yCenterAlignItemData = new PushButtonData("BTN_Y_CENTER_ALIGN", "Y-中央寄せ",
                assemblyFullName, "BIMwork.YCenterAlign");
            PushButton yCenterAlignItem = alignmentGroup.AddPushButton(yCenterAlignItemData);
            xLeftAlignItem.ToolTip = "Y-中央寄せ";
            Uri yCenterAlignUri16x16 = new Uri(getApplicationResourcesPath("y_center_alignment_16x16.png"));
            Uri yCenterAlignUri32x32 = new Uri(getApplicationResourcesPath("y_center_alignment_32x32.png"));
            yCenterAlignItem.Image = new BitmapImage(yCenterAlignUri16x16);
            yCenterAlignItem.LargeImage = new BitmapImage(yCenterAlignUri32x32);


            // X-で整列
            PushButtonData xWithAlignItemData = new PushButtonData("BTN_X_WITH_ALIGN", "X-で整列",
                assemblyFullName, "BIMwork.XWithAlign");
            PushButton xWithAlignItem = alignmentGroup.AddPushButton(xWithAlignItemData);
            xLeftAlignItem.ToolTip = "X-で整列";
            Uri xWithAlignUri16x16 = new Uri(getApplicationResourcesPath("x_with_center_alignment_32x32.png"));
            Uri xWithAlignUri32x32 = new Uri(getApplicationResourcesPath("x_with_center_alignment_32x32.png"));
            xWithAlignItem.Image = new BitmapImage(xWithAlignUri16x16);
            xWithAlignItem.LargeImage = new BitmapImage(xWithAlignUri32x32);

            // Y-で整列
            PushButtonData yWithAlignItemData = new PushButtonData("BTN_Y_WITH_ALIGN", "Y-で整列",
                assemblyFullName, "BIMwork.YWithAlign");
            PushButton yWithAlignItem = alignmentGroup.AddPushButton(yWithAlignItemData);
            xLeftAlignItem.ToolTip = "X-で整列";
            Uri yWithAlignUri16x16 = new Uri(getApplicationResourcesPath("y-with_center_alignment_32x32.png"));
            Uri yWithAlignUri32x32 = new Uri(getApplicationResourcesPath("y-with_center_alignment_32x32.png"));
            yWithAlignItem.Image = new BitmapImage(yWithAlignUri16x16);
            yWithAlignItem.LargeImage = new BitmapImage(yWithAlignUri32x32);


            alignmentGroup.AddSeparator();
        }

        private void createConnectionEndpointPanel(ref RibbonPanel panel)
        {
            // Get Assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyFullName = assembly.Location;

            PulldownButtonData connectionGroupData = new PulldownButtonData("PULLDOWN_GROUP_CONNECT", GROUP_CONNECTION_NAME);
            PulldownButton connectionGroup = panel.AddItem(connectionGroupData) as PulldownButton;
            connectionGroup.ToolTip = "端点⼀括結合許可・禁⽌";
            // group1.LongDescription = "<p>Hello,</p><p>I am ComboBox #1.</p><p>Regards,</p>";

            Uri uri16x16 = new Uri(getApplicationResourcesPath("allow_disallow_16x16.png"));
            Uri uri32x32 = new Uri(getApplicationResourcesPath("allow_disallow_32x32.png"));
            connectionGroup.Image = new BitmapImage(uri16x16);
            connectionGroup.LargeImage = new BitmapImage(uri32x32);

            // disallow
            PushButtonData disallowItemData = new PushButtonData("BTN_DISALLOW_ENDPOINT", "端点⼀括結合禁⽌",
                assemblyFullName, "BIMwork.DisallowEndpoint");
            PushButton disallowItem = connectionGroup.AddPushButton(disallowItemData);
            disallowItem.ToolTip = "端点⼀括結合禁⽌";
            Uri disallowUri16x16 = new Uri(getApplicationResourcesPath("disallow_16x16.png"));
            Uri disallowUri32x32 = new Uri(getApplicationResourcesPath("disallow_32x32.png"));
            disallowItem.Image = new BitmapImage(disallowUri16x16);
            disallowItem.LargeImage = new BitmapImage(disallowUri32x32);

            // allow
            PushButtonData allowItemData = new PushButtonData("BTN_LEADER_90", "端点⼀括結合許可",
                assemblyFullName, "BIMwork.AllowEndpoint");
            PushButton allowItem = connectionGroup.AddPushButton(allowItemData);
            allowItem.ToolTip = "端点⼀括結合許可";
            Uri allowUri16x16 = new Uri(getApplicationResourcesPath("allow_16x16.png"));
            Uri allowUri32x32 = new Uri(getApplicationResourcesPath("allow_32x32.png"));
            allowItem.Image = new BitmapImage(allowUri16x16);
            allowItem.LargeImage = new BitmapImage(allowUri32x32);

            connectionGroup.AddSeparator();
        }


        private void createCADReload(ref RibbonPanel panel)
        {
            // Get Assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyFullName = assembly.Location;

            PushButtonData btnData = new PushButtonData("BTN_CAD_RELOAD", BTN_CAD_RELOAD_NAME,
                assemblyFullName, "BIMwork.CADReload");
            btnData.ToolTip = "CAD 再ロード";
            Uri uri16x16 = new Uri(getApplicationResourcesPath("cad_16x16.png"));
            Uri uri32x32 = new Uri(getApplicationResourcesPath("cad_32x32.png"));
            btnData.Image = new BitmapImage(uri16x16);
            btnData.LargeImage = new BitmapImage(uri32x32);
            panel.AddItem(btnData);
        }

        private void createDeleteDefaultType(ref RibbonPanel panel)
        {
            // Get Assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyFullName = assembly.Location;

            PushButtonData btnData = new PushButtonData("BTN_DELETE_DEFAULT_TYPE", BTN_DELETE_DEFAULT_NAME,
                assemblyFullName, "BIMwork.DeleteDefaultType");
            btnData.ToolTip = "デフォルトタイプ削除";
            Uri uri16x16 = new Uri(getApplicationResourcesPath("delete_16x16.png"));
            Uri uri32x32 = new Uri(getApplicationResourcesPath("delete_32x32.png"));
            btnData.Image = new BitmapImage(uri16x16);
            btnData.LargeImage = new BitmapImage(uri32x32);
            panel.AddItem(btnData);
        }

        private void createBoundingBoxAligment(ref RibbonPanel panel)
        {
            // Get Assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyFullName = assembly.Location;

            PushButtonData btnData = new PushButtonData("BTN_BOUNDING_BOX_ALIGNMENT_DEFAULT", BTN_BOUNDING_BOX_ALIGNMENT_DEFAULT_NAME,
                assemblyFullName, "BIMwork.BoundingBoxAlignment");
            btnData.ToolTip = "Bounding Box Alignment";
            Uri uri16x16 = new Uri(getApplicationResourcesPath("bounding_box_16x16.png"));
            Uri uri32x32 = new Uri(getApplicationResourcesPath("bounding_box_32x32.png"));
            btnData.Image = new BitmapImage(uri16x16);
            btnData.LargeImage = new BitmapImage(uri32x32);
            panel.AddItem(btnData);
        }
    }
}
