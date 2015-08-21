using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;

namespace MotionFileConverter
{
   public partial class Form1 : Form
    {
        // MFXindex => JSONindex
        private readonly int[] JOINT_MAP = { 0, 1, 2, 3, 4, 5, 6, 7, 8, -1, -1, -1, 9, 10, 11, 12, 13, 14, 15, 16, 17, -1, -1, -1 };

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "Motion Files|*.mfx;*.json";

                if(fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBoxSource.Text = fileDialog.FileName;
                    
                    if(Path.GetExtension(fileDialog.FileName) == ".mfx")
                    {
                        buttonMfx.Enabled = false;
                        buttonJson.Enabled = true;
                    }
                    else
                    {
                        buttonMfx.Enabled = true;
                        buttonJson.Enabled = false;
                        
                    }
                }
            }
        }

        private void buttonMfx_Click(object sender, EventArgs e)
        {
            motionFileConvert(false);
        }

        private void buttonJson_Click(object sender, EventArgs e)
        {
            motionFileConvert(true);
        }

        private void motionFileConvert(bool isSourceMfx)
        {
            MemoryStream writeStream = new MemoryStream();
            SaveFileDialog dialog = new SaveFileDialog();
            FileStream stream;

            try
            {
                stream = new FileStream(textBoxSource.Text, FileMode.Open);
            }
            catch(Exception e)
            {
                MessageBox.Show("MotionFile can't read." + Environment.NewLine + e.Message);
                return;
            }
            
            // MFX => JSON
            if(isSourceMfx == true)
            {
                PLEN.MFX.XmlMfxModel source;
                PLEN.JSON.Main output = new PLEN.JSON.Main();

                dialog.Filter = "Motion Files|*.json";
                dialog.FileName = Path.GetFileNameWithoutExtension(textBoxSource.Text) + ".json";
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PLEN.MFX.XmlMfxModel));
                    source = (PLEN.MFX.XmlMfxModel)serializer.Deserialize(stream);
                    stream.Dispose();
                    if (source.Motion.Count == 0)
                        throw new Exception();
                }
                catch (Exception e)
                {
                    stream.Dispose();
                    MessageBox.Show("Deserializing Mfx file is failed." + Environment.NewLine + e.Message);
                    return;
                }

                try
                {
                    output.slot = short.Parse(source.Motion[0].ID);
                    output.name = source.Motion[0].Name;

                    source.Motion[0].Frame.Sort((a, b) => int.Parse(a.ID) - int.Parse(b.ID));

                    // Frames
                    string[] jointNames = Enum.GetNames(typeof(PLEN.JointName));
                    foreach(var frame in source.Motion[0].Frame)
                    {
                        PLEN.JSON.Frame outputFrame = new PLEN.JSON.Frame();
                        outputFrame.transition_time_ms = int.Parse(frame.Time);

                        // Joints
                        foreach (var joint in frame.Joint)
                        {
                            PLEN.JSON.Output outputJoint = new PLEN.JSON.Output();
                            
                            int jsonIndex = JOINT_MAP[int.Parse(joint.ID)];
                            if (jsonIndex >= 0)
                            {
                                outputJoint.device = jointNames[jsonIndex];
                                outputJoint.value = short.Parse(joint.Joint);

                                outputFrame.outputs.Add(outputJoint);
                            }
                        }
                        output.frames.Add(outputFrame);
                    }
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(PLEN.JSON.Main));
                    serializer.WriteObject(writeStream, output);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Converting to JSON file is failed." + Environment.NewLine + e.Message);
                    return;
                }
            }
            // JSON => MFX
            else
            {
                PLEN.JSON.Main source;
                PLEN.MFX.XmlMfxModel output = new PLEN.MFX.XmlMfxModel();
                dialog.Filter = "Motion Files|*.mfx";
                dialog.FileName = Path.GetFileNameWithoutExtension(textBoxSource.Text) + ".mfx";

                try
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(PLEN.JSON.Main));
                    source = (PLEN.JSON.Main)serializer.ReadObject(stream);
                    stream.Dispose();
                    if (source == null)
                        throw new Exception();
                }
                catch (Exception e)
                {
                    stream.Dispose();
                    MessageBox.Show("Deserializing JSON file is failed." + Environment.NewLine + e.Message);
                    return;
                }

                try
                {
                    output.Motion = new List<PLEN.MFX.TagMotionModel>();
                    output.Motion.Add(new PLEN.MFX.TagMotionModel());

                    output.Motion[0].ID = source.slot.ToString();
                    output.Motion[0].Name = source.name;

                    /*-- JSON側のExtraタグの仕様が確定していないため，空のExtraタグを作成 --*/
                    output.Motion[0].Extra = new PLEN.MFX.TagExtraModel();
                    output.Motion[0].Extra.Function = "0";
                    output.Motion[0].Extra.Param = new List<PLEN.MFX.TagParamModel>();
                    output.Motion[0].Extra.Param.Add(new PLEN.MFX.TagParamModel());
                    output.Motion[0].Extra.Param.Add(new PLEN.MFX.TagParamModel());
                    output.Motion[0].Extra.Param[0].ID = "0";
                    output.Motion[0].Extra.Param[1].ID = "1";
                    output.Motion[0].Extra.Param[0].Param = "0";
                    output.Motion[0].Extra.Param[1].Param = "0";

                    output.Motion[0].FrameNum = source.frames.Count.ToString();
                    output.Motion[0].Frame = new List<PLEN.MFX.TagFrameModel>();

                    int frameCnt = 0;
                    foreach (var frame in source.frames)
                    {
                        PLEN.MFX.TagFrameModel outputFrame = new PLEN.MFX.TagFrameModel();
                        outputFrame.ID = (frameCnt++).ToString();
                        outputFrame.Time = frame.transition_time_ms.ToString();
                        outputFrame.Joint = new List<PLEN.MFX.TagJointModel>();
                        for (int i = 0; i < JOINT_MAP.Length; i++)
                        {
                            PLEN.MFX.TagJointModel joint = new PLEN.MFX.TagJointModel();
                            joint.ID = i.ToString();
                            joint.Joint = "0";
                            outputFrame.Joint.Add(joint);
                        }

                        string[] jointNames = Enum.GetNames(typeof(PLEN.JointName));
                        foreach (var joint in frame.outputs)
                        {
                            int jsonIndex = Array.IndexOf(jointNames, joint.device);
                            int mfxIndex = Array.IndexOf(JOINT_MAP, jsonIndex);

                            if (jsonIndex >= 0 && mfxIndex >= 0)
                            {
                                outputFrame.Joint[mfxIndex].Joint = joint.value.ToString();
                            }
                        }
                        output.Motion[0].Frame.Add(outputFrame);
                    }
                    XmlSerializer serializer = new XmlSerializer(typeof(PLEN.MFX.XmlMfxModel));
                    serializer.Serialize(writeStream, output);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Converting to MFX file is failed." + Environment.NewLine + e.Message);
                    return;
                }
            }

            if (writeStream == null)
                return;

            // Converted MotionFile Save
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(dialog.FileName, writeStream.ToArray());
                MessageBox.Show("MotionFile was converted.");
            }

        }

    }
}
