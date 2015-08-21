using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLEN.MFX
{
    /// <summary>
    /// モーションファイル（XML形式）
    /// </summary>
    [System.Xml.Serialization.XmlRoot("mfx")]
    public class XmlMfxModel
    {
        [System.Xml.Serialization.XmlElement("motion")]
        public List<PLEN.MFX.TagMotionModel> Motion { get; set; }
    }
    /// <summary>
    /// モーションファイル：motionタグ
    /// </summary>
    public class TagMotionModel
    {
        [System.Xml.Serialization.XmlAttribute("id")]
        public String ID { get; set; }

        [System.Xml.Serialization.XmlElement("name")]
        public string Name { get; set; }

        [System.Xml.Serialization.XmlElement("extra")]
        public TagExtraModel Extra { get; set; }

        [System.Xml.Serialization.XmlElement("frameNum")]
        public string FrameNum { get; set; }

        [System.Xml.Serialization.XmlElement("frame")]
        public List<TagFrameModel> Frame { get; set; }
    }
    /// <summary>
    /// モーションファイル：extraタグ
    /// </summary>
    public class TagExtraModel
    {

        [System.Xml.Serialization.XmlElement("function")]
        public string Function { get; set; }

        [System.Xml.Serialization.XmlElement("param")]
        public List<TagParamModel> Param { get; set; }
    }
    /// <summary>
    /// モーションファイル：paramタグ
    /// </summary>
    public class TagParamModel
    {
        [System.Xml.Serialization.XmlAttribute("id")]
        public String ID { get; set; }

        [System.Xml.Serialization.XmlText()]
        public string Param { get; set; }
    }
    /// <summary>
    /// モーションファイル：frameタグ
    /// </summary>
    public class TagFrameModel
    {
        [System.Xml.Serialization.XmlAttribute("id")]
        public String ID { get; set; }

        [System.Xml.Serialization.XmlElement("time")]
        public string Time { get; set; }

        [System.Xml.Serialization.XmlElement("joint")]
        public List<TagJointModel> Joint { get; set; }
    }
    /// <summary>
    /// モーションファイル：jointタグ
    /// </summary>
    public class TagJointModel
    {
        [System.Xml.Serialization.XmlAttribute("id")]
        public String ID { get; set; }

        [System.Xml.Serialization.XmlText()]
        public string Joint { get; set; }
    }
    
}
