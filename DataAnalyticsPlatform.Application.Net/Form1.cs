using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using DataAnalyticsPlatform.Shared.Models;
using DataAnalyticsPlatform.Shared;
using Newtonsoft.Json;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Writers;
using DataAnalyticsPlatform.Shared.Types;
using DataAnalyticsPlatform.Actors.Master;
using DataAnalyticsPlatform.Actors.System;
using DataAnalyticsPlatform.Application.Net;
using Newtonsoft.Json.Schema;
using DataAnalyticsPlatform.Common;
using DataAnalyticsPlatform.Shared.PostModels;
using Newtonsoft.Json.Linq;

namespace ApplicationNet
{
    public partial class Form1 : Form
    {
        HashSet<string> ModelNames { get; set; }

        BindingList<FieldInfo> _dataList;

        ContextMenu mnu = new ContextMenu();
        ContextMenu nodeMnu = new ContextMenu();

        TreeNode _rightClickedNode = null;

        private string _lastSchemaId = null;

        public Form1()
        {
            InitializeComponent();

            ModelNames = new HashSet<string>();

            this.Load += Form1_Load;

            this.treeViewModels.AllowDrop = true;
            this.lstMainObject.AllowDrop = true;
            this.lstMainObject.MouseDown += LstMainObject_MouseDown;
            this.lstMainObject.DragOver += LstMainObject_DragOver;

            
            this.treeViewMainClass.ItemDrag += TreeViewMainClass_ItemDrag;
            this.treeViewMainClass.NodeMouseDoubleClick += TreeViewMainClass_NodeMouseDoubleClick;

            this.treeViewModels.NodeMouseClick += TreeViewModels_NodeMouseClick;
            this.treeViewModels.NodeMouseDoubleClick += TreeViewModels_NodeMouseDoubleClick1;
            this.treeViewModels.DragEnter += TreeViewModels_DragEnter;
            this.treeViewModels.DragDrop += TreeViewModels_DragDrop;            
            this.treeViewModels.LabelEdit = true;
            this.treeViewModels.AfterLabelEdit += TreeViewModels_AfterLabelEdit;
            this.treeViewModels.BeforeLabelEdit += TreeViewModels_BeforeLabelEdit;
            
            
            lstMainObject.SelectionMode = SelectionMode.One;

            FillContextMenu();
           
        }
      
        private void TreeViewModels_NodeMouseDoubleClick1(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                if (e.Node.Tag != null && e.Node.Tag is FieldInfo fi)
                {
                    using (FormEditData frm = new FormEditData())
                    {
                        frm.SelectedData = fi;
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            e.Node.Tag = frm.SelectedData;
                            e.Node.Text = frm.SelectedData.Name;
                        }
                    }
                }
            }
        }

        private void TreeViewMainClass_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Node.Tag != null && e.Node.Tag is FieldInfo fi)
                {
                    using (FormEditData frm = new FormEditData())
                    {
                        frm.SelectedData = fi;
                        frm.HideMap = true;
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            e.Node.Tag = frm.SelectedData;
                            e.Node.Text = frm.SelectedData.Name;
                        }
                    }
                }
            }
        }

        private void TreeViewModels_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _rightClickedNode = e.Node;
                //nodeMnu.Show(treeViewModels, e.Location);
            }          
        }

        private void FillContextMenu()
        {
            MenuItem addNewModel = new MenuItem("Add New Model");
            addNewModel.Click += AddNewModel_Click;

            MenuItem addNewObject = new MenuItem("Add New Object");
            addNewObject.Click += AddNewObject_Click;

            mnu.MenuItems.Add(addNewModel);
            nodeMnu.MenuItems.Add(addNewObject);

            treeViewModels.ContextMenu = mnu;
        }

        private void AddNewObject_Click(object sender, EventArgs e)
        {
            TreeNode newNode = new TreeNode();
            newNode.Text = "obj";
            newNode.Tag = new JSchema() { Type = JSchemaType.Object };
            _rightClickedNode.Nodes.Add(newNode);
            _rightClickedNode.Expand();

        }

        private void AddNewModel_Click(object sender, EventArgs e)
        {
            int index = treeViewModels.Nodes.Count;

            var modelName = $"Model{index + 1}";

            if (ModelNames.Add(modelName))
            {
                TreeNode node = new TreeNode(modelName);
                node.Text = modelName;
                treeViewModels.Nodes.Add(node);
                node.ContextMenu = nodeMnu;
            }
            else
            {
                MessageBox.Show($"Model with name '{modelName}' already added.");
            }
        }

        private void TreeViewMainClass_ItemDrag(object sender, ItemDragEventArgs e)
        {
            this.treeViewMainClass.DoDragDrop(e.Item, DragDropEffects.Move);
        }

       

        private void TreeViewModels_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Parent == null)
            {

            }
            else
            {
                e.CancelEdit = true;
                e.Node.EndEdit(true);
            }            
        }

        private void TreeViewModels_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {

            if (string.IsNullOrEmpty(e.Label))
            {
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                return;
            }

            if (e.Node.Text == e.Label)
            {
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                return;
            }

            if (ModelNames.Add(e.Label) == false)
            {
                e.CancelEdit = true;
                MessageBox.Show("Model name already in use.");

                e.Node.BeginEdit();
                return;
            }
            else
            {
                ModelNames.Remove(e.Node.Text);
                e.Node.EndEdit(false);
            }

           

            //throw new NotImplementedException();
        }

        private void TreeViewModels_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode nodeToDropIn = this.treeViewModels.GetNodeAt(this.treeViewModels.PointToClient(new Point(e.X, e.Y)));
            if (nodeToDropIn == null) { return; }
          
            var obj  = e.Data.GetData(typeof(TreeNode));

            if (obj is TreeNode dn)
            {
                if (dn.Nodes.Count > 1)
                {
                    if(MessageBox.Show("Drag with child nodes ?", "DAP", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        AddNewNode(nodeToDropIn, dn, true);
                    }
                }
                else
                {
                    AddNewNode(nodeToDropIn, dn, false);
                }              
            }
            else
            {

                FieldInfo data = (FieldInfo)e.Data.GetData(typeof(FieldInfo));

                if (data == null) { return; }
                var treeNode = new TreeNode(data.Name);
                treeNode.Tag = new FieldInfo(data);
                nodeToDropIn.Nodes.Add(treeNode);
                nodeToDropIn.Expand();
            }
            
        }

        private void AddNewNode(TreeNode nodeToDropIn, TreeNode dn, bool recurse)
        {
            var treeNode = new TreeNode(dn.Text);
            treeNode.Tag = dn.Tag;
            nodeToDropIn.Nodes.Add(treeNode);
            if (recurse)
            {
                foreach (TreeNode item in dn.Nodes)
                {
                    AddNewNode(treeNode, item, true);
                }
            }
            nodeToDropIn.Expand();
        }

        private void TreeViewModels_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void LstMainObject_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void LstMainObject_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 2) // double click
            {
                int index = this.lstMainObject.IndexFromPoint(e.Location);
                if (index != System.Windows.Forms.ListBox.NoMatches)
                {
                    using (FormEditData frm = new FormEditData())
                    {
                        frm.SelectedData = (FieldInfo)lstMainObject.Items[index];

                        frm.HideMap = true;

                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            _dataList[index] = frm.SelectedData;

                           

                            lstMainObject.SelectedIndex = index;
                        }
                    }                   
                }
            }
            else
            {
                this.lstMainObject.DoDragDrop(lstMainObject.SelectedItem, DragDropEffects.Move);
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            

            //LoadMainObject();

        }

        private void LoadMainObject()
        {
            #region Comment JSchema
            //JSchema schema = new JSchema();
            //schema.Type = JSchemaType.Object;
            //schema.Properties.Add("policyId", new JSchema() { Type = JSchemaType.Integer });
            //schema.Properties.Add("statecode", new JSchema() { Type = JSchemaType.Integer });
            //schema.Properties.Add("county", new JSchema() { Type = JSchemaType.String });
            //schema.Properties.Add("eq_site_limit", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("hu_site_limit", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("fl_site_limit", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("fr_site_limit", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("tiv_2011", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("tiv_2012", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("eq_site_deductible", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("hu_site_deductible", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("fl_site_deductible", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("fr_site_deductible", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("point_latitude", new JSchema() { Type = JSchemaType.Number });
            //schema.Properties.Add("point_longitude", new JSchema() { Type = JSchemaType.Number });

            //var nested = new JSchema() { Type = JSchemaType.Object };           
            //nested.Properties.Add("xPoint", new JSchema() { Type = JSchemaType.Integer });
            //nested.Properties.Add("yPoint", new JSchema() { Type = JSchemaType.Integer });

            //schema.Properties.Add("line", nested);
            //schema.Properties.Add("construction", new JSchema() { Type = JSchemaType.String });
            //schema.Properties.Add("point_granularity", new JSchema() { Type = JSchemaType.Number });

            //LoadSchema(null, schema);
            #endregion

            var fieldList = new List<FieldInfo>();

            fieldList.Add(new FieldInfo("policyID", DataType.Int));
            fieldList.Add(new FieldInfo("statecode", DataType.String));
            fieldList.Add(new FieldInfo("county", DataType.String));
            fieldList.Add(new FieldInfo("eq_site_limit", DataType.Double));
            fieldList.Add(new FieldInfo("hu_site_lim", DataType.Double));
            fieldList.Add(new FieldInfo("fl_site_limit", DataType.Double));
            fieldList.Add(new FieldInfo("fr_site_limit", DataType.Double));
            fieldList.Add(new FieldInfo("tiv_2011", DataType.Double));
            fieldList.Add(new FieldInfo("tiv_2012", DataType.Double));
            fieldList.Add(new FieldInfo("eq_site_deductible", DataType.Double));
            fieldList.Add(new FieldInfo("hu_site_deductible", DataType.Double));
            fieldList.Add(new FieldInfo("fl_site_deductible", DataType.Double));
            fieldList.Add(new FieldInfo("fr_site_deductible", DataType.Double));
            FieldInfo nested = new FieldInfo("point", DataType.Object);
            nested.AddField(new FieldInfo("point_latitude", DataType.Double));
            nested.AddField(new FieldInfo("point_longitude", DataType.Double));
            fieldList.Add(nested);
            fieldList.Add(new FieldInfo("line", DataType.String));
            fieldList.Add(new FieldInfo("construction", DataType.String));
            fieldList.Add(new FieldInfo("point_granularity", DataType.String));

            //LoadFieldInfo(null, fieldList);
        }

        private void LoadFieldInfo(TreeNode parentNode, List<FieldInfo> fieldInfoList)
        {
            foreach (var item in fieldInfoList)
            {
                TreeNode node = new TreeNode();
                node.Text = item.Name;
                node.Tag = item;

                if (item.DataType == DataType.Object)
                {
                    if (item.InnerFields != null && item.InnerFields.Count > 0)
                        LoadFieldInfo(node, item.InnerFields);
                }
                if (parentNode == null)
                {
                    treeViewMainClass.Nodes.Add(node);
                }
                else
                {
                    parentNode.Nodes.Add(node);
                }
            }
        }

        private void LoadSchema(TreeNode parentNode, JSchema schema)
        {
            foreach (var item in schema.Properties)
            {
                TreeNode node = new TreeNode();
                node.Text = item.Key;
                node.Tag = item.Value;

                if (item.Value.Type == JSchemaType.Object)
                {
                    LoadSchema(node, item.Value);
                }
                if (parentNode == null)
                {
                    treeViewMainClass.Nodes.Add(node);
                }
                else
                {
                    parentNode.Nodes.Add(node);
                }                
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if(dialog.ShowDialog()== DialogResult.OK)
                {
                    txtSourceFile.Text = dialog.FileName;
                }
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            string sourceFile = txtSourceFile.Text;

            string url = txtUrl.Text;

            var schemaModel = Post(url, sourceFile);

            LoadFieldInfo(null, schemaModel.ListOfFieldInfo);

        }


        private static JToken Post(string url, PreviewUpdate ur)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(ur);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    return JToken.Parse(result);                    
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

            return null;
        }

        private static JToken Post(string url, LoadRequest lr)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(lr);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    return JToken.Parse(result);                    
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

            return null;
        }

        private static SchemaModel Post(string url, string fileName)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(new PreviewRequest(fileName));
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    return JsonConvert.DeserializeObject<SchemaModel>(result);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

       
        private void btnNewModel_Click(object sender, EventArgs e)
        {
            int index = treeViewModels.Nodes.Count;

            var modelName = $"Model{index + 1}";

            if (ModelNames.Add(modelName))
            {
                TreeNode node = new TreeNode(modelName);
                node.Text = modelName;
                treeViewModels.Nodes.Add(node);
                node.ContextMenu = nodeMnu;
            }
            else
            {
                MessageBox.Show($"Model with name '{modelName}' already added.");
            }

        }

        private void btnCreateReadCfg_Click(object sender, EventArgs e)
        {
            TypeConfig tc = GetTypeConfigFromUI();

            var json  = JsonConvert.SerializeObject(tc, Formatting.Indented);

            //txtXml.Text = json;
        }

        private TypeConfig GetTypeConfigFromUI()
        {
            var typeConfig = new TypeConfig();

            typeConfig.BaseClassFields = new List<FieldInfo>();

            foreach (TreeNode node in treeViewMainClass.Nodes)
            {
                if (node.Tag != null && node.Tag is FieldInfo fi)
                {
                    var fieldInfo = GetFieldInfo(node, fi);

                    typeConfig.BaseClassFields.Add(fieldInfo);

                }
            }

            List<JSchema> splitSchema = new List<JSchema>();

            foreach (TreeNode node in treeViewModels.Nodes)
            {
                if (node.Nodes.Count > 0)
                {
                    var modelInfo = new ModelInfo();
                    modelInfo.ModelName = node.Text;
                    foreach (TreeNode childNode in node.Nodes)
                    {
                        if (childNode.Tag != null && childNode.Tag is FieldInfo fi)
                        {
                            var fieldInfo = GetFieldInfo(childNode, fi);
                            modelInfo.ModelFields.Add(fieldInfo);
                        }
                    }
                    typeConfig.ModelInfoList.Add(modelInfo);
                }
            }

            return typeConfig;
        }

        private FieldInfo GetFieldInfo(TreeNode node, FieldInfo fi)
        {            
            FieldInfo nf = new FieldInfo();
            nf.Name = fi.Name;
            nf.Length = fi.Length;
            nf.Map = fi.Map;
            if (node.Nodes != null && node.Nodes.Count > 0)
            {
                nf.DataType = DataType.Object;

                foreach (TreeNode innerNode in node.Nodes)
                {
                    if (innerNode.Tag != null && innerNode.Tag is FieldInfo fni)
                        nf.AddField(GetFieldInfo(innerNode, fni));
                }
            }

            return nf;
        }

        private int currentIndex = -1;

        private Dictionary<string, List<BaseModel>> _result = null;

        private void btnUpdateModel_Click(object sender, EventArgs e)
        {
            string url = @"http://localhost:50926/api/Preview/1/updatemodel";

                   
            var tc = GetTypeConfigFromUI();

            var jToken = Post(url, new PreviewUpdate(txtSourceFile.Text, tc));

            if (jToken != null)
            {
                PreviewUpdateResponse pur = JsonConvert.DeserializeObject<PreviewUpdateResponse>(jToken.ToString());

                _lastSchemaId = pur.SchemaId;

                DisplayTreeView(jToken, "MappedModels");
            }
            else
            {
                MessageBox.Show("Error !!!");
            }
        }

        private void DisplayTreeView(JToken root, string rootName)
        {
            treeViewResult.BeginUpdate();
            try
            {
                treeViewResult.Nodes.Clear();
                var tNode = treeViewResult.Nodes[treeViewResult.Nodes.Add(new TreeNode(rootName))];
                tNode.Tag = root;

                AddNode(root, tNode);

                treeViewResult.ExpandAll();
            }
            finally
            {
                treeViewResult.EndUpdate();
            }
        }

        private void AddNode(JToken token, TreeNode inTreeNode)
        {
            if (token == null)
                return;
            if (token is JValue)
            {
                var childNode = inTreeNode.Nodes[inTreeNode.Nodes.Add(new TreeNode(token.ToString()))];
                childNode.Tag = token;
            }
            else if (token is JObject)
            {
                var obj = (JObject)token;
                foreach (var property in obj.Properties())
                {
                    var childNode = inTreeNode.Nodes[inTreeNode.Nodes.Add(new TreeNode(property.Name))];
                    childNode.Tag = property;
                    AddNode(property.Value, childNode);
                }
            }
            else if (token is JArray)
            {
                var array = (JArray)token;
                for (int i = 0; i < array.Count; i++)
                {
                    var childNode = inTreeNode.Nodes[inTreeNode.Nodes.Add(new TreeNode(i.ToString()))];
                    childNode.Tag = array[i];
                    AddNode(array[i], childNode);
                }
            }
            else
            {
                //Debug.WriteLine(string.Format("{0} not implemented", token.Type)); // JConstructor, JRaw
            }
        }

        private void LoadResult()
        {
            currentIndex++;

            int index = 0;

            foreach (var item in _result)
            {
                if(index == currentIndex)
                {
                    //grdResult.DataSource = item.Value;
                    currentIndex = index;
                    break;
                }

                index++;                              
            }            
        }

        private void btnNext_Click(object sender, EventArgs e)
        {          
            LoadResult();

            if (currentIndex == _result.Count - 1) currentIndex = -1;
        }

        private void btnImportData_Click(object sender, EventArgs e)
        {
            string url = @"http://localhost:50926/api/LoadData/1/loadmodel";


            var jToken = Post(url, new LoadRequest(txtSourceFile.Text, _lastSchemaId));

            if (jToken != null)
            {

            }

        }
    }

     public class PreviewUpdateResponse
    {
        public string SchemaId { get; set; }
        public Dictionary<string, List<BaseModel>> ModelsPreview { get; set; }
    }
}
