﻿using Microsoft.SharePoint;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SPMin.Controls
{
    public class AssetRegistrationControl : WebControl
    {
        [Category("Settings")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string FilePath
        {
            get { return Convert.ToString(ViewState["FilePath"]) ?? ""; }
            set { ViewState["FilePath"] = value; }
        }

        protected string GetFinalPath(Environment environment)
        {
            string path = FilePath;

            if (environment == Environment.Production)
            {
                string fileName = Path.GetFileName(FilePath);
                var fileNameParser = new FileNameParser(fileName);

                if (fileNameParser.ShouldBeMinified)
                {
                    string directoryPath = FileUtilities.GetDirectoryPathFromFilePath(path);
                    path = String.Format("{0}/{1}", directoryPath, fileNameParser.MinifiedVersionFileName);
                }
            }

            path = String.Format("{0}/Style Library/{1}", SPContext.Current.Web.ServerRelativeUrl, path);
            path = FileUtilities.RemoveDuplicatedSlashesFromPath(path);

            return path;
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            Environment environment = EnvironmentDetector.Detect(SPContext.Current.Site);
            var html = new StringBuilder();

            if (environment == Environment.Development)
                GenerateIncludeScriptTags(html);

            string finalFilePath = GetFinalPath(environment);
            html.AppendLine(GenerateHtml(finalFilePath));

            output.Write(html.ToString());
        }

        protected void GenerateIncludeScriptTags(StringBuilder html)
        {
            SPWeb web = SPContext.Current.Site.RootWeb;

            string filePath = String.Format("{0}/Style Library/{1}", web.ServerRelativeUrl, FilePath);
            filePath = FileUtilities.RemoveDuplicatedSlashesFromPath(filePath);

            try
            {
                SPFile file = web.GetFile(filePath);
                var reader = new FileReader(file);
                string fileDirectory = FileUtilities.GetDirectoryPathFromFilePath(filePath);
             
                foreach (string includedFile in reader.IncludedFiles)
                    html.AppendLine(GenerateHtml(fileDirectory + "/" + includedFile));
            }
            catch (SPException)
            {
                html.AppendLine("<!-- SPMin: " + HttpUtility.HtmlEncode(filePath) + " not found -->");
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.Write("");
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.Write("");
        }

        public virtual string GenerateHtml(string path)
        {
            throw new NotImplementedException();
        }
    }
}