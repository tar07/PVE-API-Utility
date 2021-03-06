﻿/*
 * CustomExtension.cs
 * Extensions for sending / searching XML strings and adding color support for RichTextBoxes.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.String;

namespace PVEAPIUtility
{
    namespace CustomExtensions
    {
        /// <summary>
        /// Extensions for sending / searching XML strings and adding color support for RichTextBoxes
        /// </summary>
        public static class CustomExtensions
        {
            /// <summary>
            /// Appends supplied text to RichTextBox with the specified color.
            /// </summary>
            /// <param name="box"></param>
            /// <param name="text"></param>
            /// <param name="color"></param>
            public static void AppendText(this RichTextBox box, string text, Color color)
            {
                box.SelectionStart = box.TextLength;
                box.SelectionLength = 0;
                box.SelectionColor = color;
                box.AppendText(text);
                box.SelectionColor = box.ForeColor;
            }

            /// <summary>
            /// Replaces only the first occurrence of the search text in the string.
            /// </summary>
            /// <param name="text"></param>
            /// <param name="search"></param>
            /// <param name="replace"></param>
            /// <returns></returns>
            public static string ReplaceFirst(this string text, string search, string replace)
            {
                int pos = text.IndexOf(search);
                if (pos < 0)
                {
                    return text;
                }

                return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
            }

            /// <summary>
            /// Send an XML query to the server.
            /// </summary>
            /// <param name="xmlString">XML-formatted query to send.</param>
            /// <param name="hostURL">URL to send the query to.</param>
            /// <returns></returns>
            public static async Task<string> SendXml(this string xmlString, string hostURL)
            {
                if (IsNullOrEmpty(hostURL)) throw new ArgumentNullException("hostURL");

                using (var client = new HttpClient())
                {
                    hostURL = APIHelper.SanitizeURL(hostURL);
                    try
                    {
                        using (var query = new StringContent(xmlString, Encoding.UTF8, "application/xml"))
                        {
                            var response = await client.PostAsync(hostURL, query);
                            var result = await response.Content.ReadAsStringAsync();
                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message != null && e.InnerException != null)
                            throw new Exception($"***ERROR***\n{e.Message}\n{e.InnerException.Message}");
                        else if (e.Message != null)
                            throw new Exception($"***ERROR***\n{e.Message}");
                        return null;
                    }
                }
            }

            /// <summary>
            /// Parses XML in the form of a string, searches for a specific XML node and returns the value of the XML node.
            /// </summary>
            /// <param name="xmlString"></param>
            /// <param name="xmlNode"></param>
            /// <returns>Empty string on failure.</returns>
            public static string TryGetXmlNode(this string xmlString, string xmlNode, out bool success)
            {
                success = true;
                try
                {
                    using (var xml = new StringReader(xmlString))
                    {
                        using (var reader = XmlReader.Create(xml))
                        {
                            if (reader.ReadToFollowing(xmlNode))
                            {
                                return reader.ReadElementContentAsString().Trim();
                            }
                            else
                            {
                                return Empty;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    success = false;
                    return Empty;
                }
            }

            /// <summary>
            /// Returns all values for a particular node.
            /// </summary>
            /// <param name="xmlString"></param>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static List<string> TryGetXmlNodes(this string xmlString, string xmlNode, out bool success)
            {
                success = false;
                List<string> nodeValues = new List<string>();
                try
                {
                    using (var xmlSR = new StringReader(xmlString))
                    {
                        using (XmlReader reader = XmlReader.Create(xmlSR))
                        {
                            while (reader.ReadToFollowing(xmlNode))
                            {
                                success = true;
                                nodeValues.Add(reader.ReadElementContentAsString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return nodeValues;
            }
        }
    }
}