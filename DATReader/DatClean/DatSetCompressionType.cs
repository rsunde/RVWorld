﻿using System.Collections.Generic;
using DATReader.DatStore;

namespace DATReader.DatClean
{
    public class DatSetCompressionType
    {
        // added this extra set, so that this was no longer using the static _parents list
        // so that this function could be called in parellel.
        public static void SetZip(DatBase inDat, bool is7zip = false)
        {
            List<DatDir> parents = new List<DatDir>();
            SetZip(parents, inDat, is7zip);
        }

        private static void SetZip(List<DatDir> parents, DatBase inDat, bool is7Zip = false)
        {
            int parentCount = parents.Count;

            if (inDat is DatFile dFile)
            {
                if (dFile.isDisk)
                {
                    //go up 2 levels to find the directory of the game
                    DatDir dir = parents[parentCount - 2];
                    DatDir zipDir = parents[parentCount - 1];

                    DatDir tmpFile = new DatDir(DatFileType.Dir)
                    { Name = zipDir.Name, DGame = zipDir.DGame };

                    if (dir.ChildNameSearch(tmpFile, out int index) != 0)
                    {
                        dir.ChildAdd(tmpFile);
                    }
                    else
                    {
                        tmpFile = (DatDir)dir.Child(index);
                    }
                    dFile.DatFileType = DatFileType.File;
                    tmpFile.ChildAdd(dFile);

                }
                else
                {
                    dFile.Name = dFile.Name.Replace("\\", "/");
                    dFile.DatFileType = is7Zip ? DatFileType.File7Zip : DatFileType.FileTorrentZip;
                    parents[parentCount - 1].ChildAdd(dFile);
                }
                return;
            }

            if (!(inDat is DatDir dDir))
                return;

            if (parents.Count > 0)
                parents[parentCount - 1].ChildAdd(inDat);

            dDir.DatFileType = dDir.DGame == null ?
                DatFileType.Dir :
                (is7Zip ? DatFileType.Dir7Zip : DatFileType.DirTorrentZip);

            DatBase[] children = dDir.ToArray();
            if (children == null)
                return;

            dDir.ChildrenClear();

            parents.Add(dDir);
            foreach (DatBase child in children)
            {
                SetZip(parents, child, is7Zip);
            }
            parents.RemoveAt(parentCount);
        }


        public static void SetFile(DatBase inDat)
        {
            if (inDat is DatFile dFile)
            {
                dFile.DatFileType = DatFileType.File;
                return;
            }

            if (!(inDat is DatDir dDir))
                return;

            if (dDir.DGame == null)
            {
                dDir.DatFileType = DatFileType.Dir;
            }
            else
            {
                dDir.DatFileType = DatFileType.Dir;
            }

            DatBase[] children = dDir.ToArray();
            if (children == null)
                return;

            dDir.ChildrenClear();

            foreach (DatBase child in children)
            {
                SetFile(child);
                dDir.ChildAdd(child);
            }

        }

    }
}
