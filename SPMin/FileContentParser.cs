﻿using Microsoft.SharePoint;
using System.Linq;
using System.Text;
using System;

namespace SPMin
{
    public class FileContentParser
    {
        private SPFile mainFile;
        private SPFolder parentFolder;
        private string mainContent;

        public string[] IncludedFiles { get; private set; }

        public FileContentParser(SPFile file)
        {
            this.mainFile = file;
            this.parentFolder = file.ParentFolder;
            this.mainContent = GetContent(mainFile);
            
            IncludedFiles = FileInclusionParser.GetIncludedFiles(mainContent);
        }

        public string GetCompiledContent()
        {
            var finalContent = new StringBuilder();

            foreach (string includedFileName in this.IncludedFiles)
            {
                SPFile includedFile = FileUtilities.GetFile(parentFolder, includedFileName);

                if (includedFile != null && includedFile.Exists)
                    finalContent.AppendLine(GetContent(includedFile) + FileSeparator);
            }

            finalContent.AppendLine(mainContent);

            return finalContent.ToString();
        }

        private string GetContent(SPFile file)
        {
            string content = Encoding.UTF8.GetString(RemoveBOM(file.OpenBinary()));

            if (file.Name.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                content = new CssRelativePathResolver(file.ServerRelativeUrl, content).Resolve();

            return content;
        }

        private byte[] RemoveBOM(byte[] bytes)
        {
            byte[] preamble = Encoding.UTF8.GetPreamble();

            if (bytes.Take(preamble.Length).SequenceEqual(preamble))
                bytes = bytes.Skip(preamble.Length).ToArray();

            return bytes;
        }

        private string FileSeparator
        {
            get
            {
                return (mainFile.Name.EndsWith(".js", StringComparison.InvariantCultureIgnoreCase)) ? ";" : "";
            }
        }
    }
}
