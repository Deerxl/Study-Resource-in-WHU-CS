using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Xml.Schema;

namespace xml
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //创建一个XML文档
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<bookstore/>");
            //创建XML版本声明.
            XmlDeclaration xmldecl;
            xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            //根节点
            XmlNode root = xmlDoc.DocumentElement;
            xmlDoc.InsertBefore(xmldecl, root);
            //创建一个节点
            XmlElement xe1 = xmlDoc.CreateElement("book");
            //设置该节点genre属性
            xe1.SetAttribute("genre", "黄敏");
            //设置该节点ISBN属性
            xe1.SetAttribute("ISBN", "2-3631-4");
            XmlElement xesub1 = xmlDoc.CreateElement("title");
            xesub1.InnerText = "CS从入门到精通";//设置文本节点
            xe1.AppendChild(xesub1);//添加到节点中
            XmlElement xesub2 = xmlDoc.CreateElement("author");
            xesub2.InnerText = "候捷";
            xe1.AppendChild(xesub2);
            XmlElement xesub3 = xmlDoc.CreateElement("price");
            xesub3.InnerText = "58.3";
            xe1.AppendChild(xesub3);
            root.AppendChild(xe1);//添加到节点中
            xmlDoc.Save("bookstore.xml");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.Filter = "XML文档|*.xml";
            StreamReader sr = null;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                xml_filename = openFileDialog1.FileName;
                sr = new StreamReader(xml_filename);
                textBox1.Text = sr.ReadToEnd();
                sr.Close();
            }
        }
        public static string xml_filename;
        private void button4_Click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            if (File.Exists(xml_filename))
            {
                doc.Load(xml_filename);
                treeView1.Nodes.Clear();
                XmlNode root = doc.DocumentElement;
                TreeNode node;
                node = treeView1.Nodes.Add("XML文档");
                build_tree(root, node); treeView1.ExpandAll();
            }
        }
        private void build_tree(XmlNode xnode, TreeNode p_tnode)
        {
            TreeNode node;
            switch (xnode.NodeType)
            {
                case XmlNodeType.Element:
                    {
                        node = p_tnode.Nodes.Add(xnode.Name);
                        if (xnode.HasChildNodes)
                        {
                            for (int i = 0; i < xnode.ChildNodes.Count; i++)
                            {
                                build_tree(xnode.ChildNodes[i], node);
                            }
                        }
                        break;
                    }
                case XmlNodeType.Text:
                    {
                        p_tnode.Nodes.Add(xnode.Value);
                        break;
                    }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("bookstore.xml");
            //xmlDoc.SelectNodes
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("bookstore").ChildNodes;//获取 book-store节点的所有子节点
            foreach (XmlNode xn in nodeList)//遍历所有子节点
            {
                XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                if (xe.GetAttribute("genre") == "黄敏")//如果genre属性值为“黄敏”
                {
                    xe.SetAttribute("genre", "update黄敏");//则修改该属性为“update黄敏”
                    XmlNodeList nls = xe.ChildNodes;//继续获取xe子节点的所有子节点
                    foreach (XmlNode xn1 in nls)//遍历
                    {
                        XmlElement xe2 = (XmlElement)xn1;//转换类型
                        if (xe2.Name == "author")//如果找到
                        {
                            xe2.InnerText = "亚胜";//则修改
                            break;//找到退出来就可以了
                        }
                    }
                    break;
                }
            }
            xmlDoc.Save("bookstore.xml");//保存xml文件。
        }


        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            MessageBox.Show(String.Format("Validation Error: {0}", e.Message));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("urn:bookstore-schema", "books.xsd");
            XmlReaderSettings settings = new XmlReaderSettings();
            //ConformanceLevel = ConformanceLevel.Document
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = sc;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            XmlReader reader = XmlReader.Create(xml_filename, settings);
            // Parse the file.
            while (reader.Read()) ;
        }
    }
}
