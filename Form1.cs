using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiblioEditor
{
    public partial class Form1 : Form
    {
        // 将marcrecord对象保存在内存里，用于修改marc数据
        MarcRecord _record = null;

        public Form1()
        {
            InitializeComponent();
        }

        // 新创建一条marc
        private void button_new_Click(object sender, EventArgs e)
        {
            // 新new一个MarcRecord对象，设上头标区，0/5与12/5由软件自动生成
            this._record = new MarcRecord("?????nam0 22?????   450 ");

            // 把输入框的信息设到marc里
            this.ChangeMarc(this._record);

            #region 对于新建一条marc，如果缺少一些基础字段，自动填上

            string strNow = DateTime.Now.ToString("yyyyMMdd");


            // 增加100 // 通过处理数据 a是记录生成时间，d是出版年
            MarcNodeList nodes = this._record.select("field[@name=100]");
            if (nodes.count == 0)
            {
                string strPublishYear = this._record.select("field[@name=210]/subfield[@name='d']").FirstContent;
                if (String.IsNullOrEmpty(strPublishYear) == false && strPublishYear.Length >= 4)
                    strPublishYear = strPublishYear.Substring(0, 4);
                else
                    strPublishYear = strNow.Substring(0, 4);
                this._record.append(String.Format("100  {0}a{1}d{2}    ekmy0chiy50      ea",
                    MarcQuery.SUBFLD, strNow, strPublishYear));
            }

            // 增加 101 作品语种
             nodes = this._record.select("field[@name=101]");
            if (nodes.count == 0)
                this._record.append(String.Format("101 0{0}achi", MarcQuery.SUBFLD));

            // 增加 102 出版国
            nodes = this._record.select("field[@name=102]");
            if (nodes.count == 0)
                this._record.append(String.Format("102  {0}aCN", MarcQuery.SUBFLD));

            // 105 编辑数据字段
            nodes = this._record.select("field[@name=105]");
            if (nodes.count == 0)
                this._record.append(String.Format("105  {0}ay   z   000yy", MarcQuery.SUBFLD));

            // 106 编辑数据字段
            nodes = this._record.select("field[@name=106]");
            if (nodes.count == 0)
                this._record.append(String.Format("106  {0}ar", MarcQuery.SUBFLD));

            // 801
            nodes = this._record.select("field[@name=801]");
            if (nodes.count == 0)
                this._record.append(String.Format("801 aCN{0}c{1}", MarcQuery.SUBFLD, strNow));

            //// 批次号
            //if (String.IsNullOrEmpty(BatchNo) == false)
            //    record.append(String.Format("998  {0}a{1}", MarcQuery.SUBFLD, BatchNo));

            #endregion

            // 把marc转成xml显示出来
            this.Convert2MarcXml(this._record);
        }


        // 把右侧的xml加载到左侧输入框
        private void button_load_Click(object sender, EventArgs e)
        {
            string strMarcXml = this.textBox_xml.Text.Trim();
            if (string.IsNullOrEmpty(strMarcXml) == true)
            {
                MessageBox.Show(this, "请先在右侧输入要装载marcxml");
                return;
            }

            // 将xml转为工作单格式
            string strError = "";
            string strMARC = "";
            int nRet = MarcUtil.Xml2Marc(strMarcXml,
                false,
                "",
                out string strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet != 0)
            {
                strError = "MarcXml转换错误:" + strError;
            }

            // 放在内存里
            this._record = new MarcRecord(strMARC);

            // 把一些字段显示在界面上
            this.ViewFields(this._record);
        }


        // 编辑保存左侧修改的内容
        private void button_save_Click(object sender, EventArgs e)
        {
            if (this._record == null)
            {
                MessageBox.Show(this, "请先装载marcXml");
                return;
            }

            // 把输入框的信息设到marc里
            this.ChangeMarc(this._record);

            // 把marc转成xml显示出来
            this.Convert2MarcXml(this._record);
        }



        /*
ISBN	010##a
价格	010##d
题名	2001#a
第一作者	2001#f

出版地	210##a
出版社	210##c
出版时间	210##d

页数	215##a
尺寸	215##d
摘要	330##a
主题词	606##a
分类号	690##a

个人主要作者 701##a

 */
        // 得到界面编辑的数据
        private List<FieldItem> GetFields()
        {
            List<FieldItem> list = new List<FieldItem>();

            // 先装到集合中，一会统一处理
            list.Add(new FieldItem { name = "010##a", value = this.textBox_isbn.Text.Trim() });
            list.Add(new FieldItem { name = "010##d", value = this.textBox_price.Text.Trim() });
            list.Add(new FieldItem { name = "2001#a", value = this.textBox_title.Text.Trim() });
            list.Add(new FieldItem { name = "2001#f", value = this.textBox_author.Text.Trim() });

            list.Add(new FieldItem { name = "210##a", value = this.textBox_publishPlace.Text.Trim() });
            list.Add(new FieldItem { name = "210##c", value = this.textBox_publisher.Text.Trim() });
            list.Add(new FieldItem { name = "210##d", value = this.textBox_publishTime.Text.Trim() });

            list.Add(new FieldItem { name = "215##a", value = this.textBox_page.Text.Trim() });
            list.Add(new FieldItem { name = "215##d", value = this.textBox_size.Text.Trim() });
            list.Add(new FieldItem { name = "606##a", value = this.textBox_subject.Text.Trim() });
            list.Add(new FieldItem { name = "690##a", value = this.textBox_clcclass.Text.Trim() });
            list.Add(new FieldItem { name = "330##d", value = this.textBox_info.Text.Trim() });

            list.Add(new FieldItem { name = "701##a", value = this.textBox_701_1.Text.Trim() });


            return list;
        }

        // 把一些字段显示在界面上
        private void ViewFields(MarcRecord record)
        {
            // 把几个需要编译的字段显示出来
            this.textBox_isbn.Text = record.select("field[@name=010]/subfield[@name='a']").FirstContent;
            this.textBox_price.Text = record.select("field[@name=010]/subfield[@name='d']").FirstContent;
            this.textBox_title.Text = record.select("field[@name=200]/subfield[@name='a']").FirstContent;
            this.textBox_author.Text = record.select("field[@name=200]/subfield[@name='f']").FirstContent;

            this.textBox_publishPlace.Text = record.select("field[@name=210]/subfield[@name='a']").FirstContent;
            this.textBox_publisher.Text = record.select("field[@name=210]/subfield[@name='c']").FirstContent;
            this.textBox_publishTime.Text = record.select("field[@name=210]/subfield[@name='d']").FirstContent;

            this.textBox_page.Text = record.select("field[@name=215]/subfield[@name='a']").FirstContent;
            this.textBox_size.Text = record.select("field[@name=215]/subfield[@name='d']").FirstContent;
            this.textBox_subject.Text = record.select("field[@name=606]/subfield[@name='a']").FirstContent;
            this.textBox_clcclass.Text = record.select("field[@name=690]/subfield[@name='a']").FirstContent;
            this.textBox_info.Text = record.select("field[@name=330]/subfield[@name='d']").FirstContent;

            this.textBox_701_1.Text = record.select("field[@name=701]/subfield[@name='a']").FirstContent;

        }

        // 根据界面输入的信息修改marc
        private void ChangeMarc(MarcRecord record)
        {
            // 获取界面编辑的数据
            List<FieldItem> list = this.GetFields();

            // 把界面输入框的字段加到marcrecord
            foreach (FieldItem item in list)
            {
                // 字段名或内容为空，则不处理
                if (String.IsNullOrEmpty(item.name) || String.IsNullOrEmpty(item.value))
                    continue;

                // 因name格式为2001#a，拆分出字段名/指示符/子字段名
                string strFieldName = item.name.Substring(0, 3);  //字段名
                string strInd = item.name.Substring(3, 2).Replace("#", " ");  //指示符
                string strSubfieldName = item.name.Substring(5);  // 子字段名

                //内容
                string strContent = item.value;

                // 先用字段名查一下有没有这个字段
                MarcNodeList fields = record.select(String.Format("field[@name={0}]", strFieldName));
                if (fields.count == 0) //没有这个字段
                {
                    if (String.IsNullOrEmpty(strSubfieldName))
                    {
                        //直接是字段的情况
                        record.append(String.Format("{0}{1}", strFieldName, strContent));
                    }
                    else
                    {
                        // 字段+子字段的情况
                        record.append(String.Format("{0}{1}{2}{3}{4}",
                            strFieldName,
                            strInd,
                            MarcQuery.SUBFLD,
                            strSubfieldName,
                            strContent));
                    }
                }
                else
                {
                    // 检查一下有没有子字段，注意只针对命中的第1个字段
                    MarcNodeList subfields = fields[0].select("subfield[@name='" + strSubfieldName + "']");
                    if (subfields.count == 0)
                        fields[0].append(String.Format("{0}{1}{2}", MarcQuery.SUBFLD, strSubfieldName, strContent));
                    else
                        subfields[0].Content = strContent;
                }
            }
        }

        // 将marcrecord转出xml,显示出来
        private void Convert2MarcXml(MarcRecord record)
        {
            // 将字段排序
            record.ChildNodes.sort();
            string strMarc = record.Text;

            // 将marc工作单转为marcxml
            string strError = "";
            string outXml = "";
            int nRet = 0;
            string strXml = "";
            nRet = MarcUtil.Marc2Xml(strMarc,
                "unimarc",//写死unimarc，//this._marcSyntax,
                out strXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "marc转成xml出错：" + strError);
                return;
            }

            // 把xml缩进显示
            nRet = DomUtil.GetIndentXml(strXml, out outXml, out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "格式化xml出错：" + strError);
                return;
            }

            this.textBox_xml.Text = outXml;
        }
    }



    // 把界面上的输入内容做成一个结构，方便用集合处理
    public class FieldItem
    {
        // 200##a这样的结构
        public string name { get; set; }

        public string value { get; set; }
    }
}
