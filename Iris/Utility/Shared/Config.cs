using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Drawing;

namespace Iris.Utility.Shared
{
    public class Config
    {
        private const int HEADERLEN = 12;
        private const int HEADERCOLORLEN = HEADERLEN / 3;
        private const int FILESIZESTART = 0;    // to 3

        public enum RemoveFlag
        {
            OBJ,
            BIN,
            Hide,
            DotFolder,
            packages,
            AllCondition,
        }
        private enum checkMode
        {
            File,
            Directory,
        }
        private delegate bool RemodeConditionCheck(string targetpath, checkMode m);
        #region CheckFunc
        private static bool ObjCheck(string targetpath, checkMode m)
        {
            bool ret = false;
            if (m == checkMode.Directory)
            {
                if ((new DirectoryInfo(targetpath)).Name.ToLower() == ("obj") || (new DirectoryInfo(targetpath)).Name.ToLower() == ("debug"))
                {
                    ret = true;
                }
            }
            else if (m == checkMode.File)
            {
            }
            return ret;
        }

        private static bool BinCheck(string targetpath, checkMode m)
        {
            bool ret = false;
            if (m == checkMode.Directory)
            {
                if ((new DirectoryInfo(targetpath)).Name.ToLower() == ("bin"))
                {
                    ret = true;
                }
            }
            return ret;
        }

        private static bool HideCheck(string targetpath, checkMode m)
        {
            bool ret = false;
            switch (m)
            {
                case checkMode.File:
                    FileInfo finfo = new FileInfo(targetpath);
                    if ((finfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        ret = true;
                    }
                    if ((new FileInfo(targetpath)).Extension.ToLower() == (".sdf"))
                    {
                        ret = true;
                    }
                    break;
                case checkMode.Directory:
                    DirectoryInfo dinfo = new DirectoryInfo(targetpath);
                    if ((dinfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        ret = true;
                    }
                    break;
                default:
                    break;
            }
            return ret;
        }

        private static bool packagesCheck(string targetpath, checkMode m)
        {
            bool ret = false;
            if (m == checkMode.Directory)
            {
                if ((new DirectoryInfo(targetpath)).Name.Contains("packages"))
                {
                    ret = true;
                }
            }
            return ret;
        }

        private static bool DotFCheck(string targetpath, checkMode m)
        {
            bool ret = false;
            if (m == checkMode.Directory)
            {
                if ((new DirectoryInfo(targetpath)).Name.ToLower().Contains("."))
                {
                    ret = true;
                }
            }
            return ret;
        }

        private static bool AllCheck(string targetpath, checkMode m)
        {
            bool ret = false;
            ret |= ObjCheck(targetpath, m);
            ret |= BinCheck(targetpath, m);
            ret |= HideCheck(targetpath, m);
            ret |= packagesCheck(targetpath, m);
            ret |= DotFCheck(targetpath, m);
            return ret;
        }
        #endregion

        static string targetLocation = string.Empty;
        static DirectoryInfo targetInfomation;

        public static string Location
        {
            get
            {
                return targetLocation;
            }
            set
            {
                targetLocation = value;
                if (targetLocation != string.Empty)
                {
                    targetInfomation = new DirectoryInfo(targetLocation);
                }
                else
                {
                    targetInfomation = null;
                }
            }
        }
        public static DirectoryInfo LocationInfo
        {
            get { return targetInfomation; }
        }
        public static string TemporaryZipLocation
        {
            get
            {
                if (LocationInfo != null)
                {
                    return LocationInfo.Parent.FullName + @"\La+.zip";
                }
                else { return string.Empty; }
            }
        }
        public static string Picture { get; private set; }

        private static byte[] Chromosome { get; set; }
        private static Color[] ColorChromosome { get; set; }

        #region Convert To Map

        public static bool CopyTarget(string dist)
        {
            bool ret = false;
            if (LocationInfo != null)
            {
                DirectoryInfo dinfo = new DirectoryInfo(dist);
                if (!dinfo.Exists || !LocationInfo.Exists)
                {
                    ret = false;
                }
                else
                {
                    CopyDirectory(targetLocation, dist + @"\" + LocationInfo.Name);
                    Location = dist + @"\" + LocationInfo.Name;
                    ret = true;
                }
            }
            return ret;
        }

        public static void ArrangementTarget(RemoveFlag Flag)
        {
            RemodeConditionCheck checkf = null;
            switch (Flag)
            {
                case RemoveFlag.OBJ:
                    checkf = ObjCheck;
                    break;
                case RemoveFlag.BIN:
                    checkf = BinCheck;
                    break;
                case RemoveFlag.Hide:
                    checkf = HideCheck;
                    break;
                case RemoveFlag.DotFolder:
                    checkf = DotFCheck;
                    break;
                case RemoveFlag.packages:
                    checkf = packagesCheck;
                    break;
                case RemoveFlag.AllCondition:
                    checkf = AllCheck;
                    break;
                default:
                    break;
            }
            if (checkf != null)
            {
                DeleteTarget(checkf, targetInfomation);
            }
        }

        public static void Compress()
        {
            //圧縮するフォルダのパス
            string folderPath = Location;
            string remove = LocationInfo.Parent.FullName;
            if (System.IO.File.Exists(TemporaryZipLocation))
            {
                System.IO.File.Delete(TemporaryZipLocation);
            }
            ZipFile.CreateFromDirectory(folderPath, TemporaryZipLocation, CompressionLevel.Optimal, true, Encoding.GetEncoding("shift_jis"));
        }

        public static void ConvertToChromosome(byte password = 0)
        {
            if (TemporaryZipLocation != string.Empty)
            {
                using (FileStream fs = new FileStream(TemporaryZipLocation, FileMode.Open, FileAccess.Read))
                {
                    Chromosome = new byte[fs.Length];
                    fs.Read(Chromosome, 0, Chromosome.Length);
                }
            }
            for (int i = 0; i < Chromosome.Length; i++)
            {
                Chromosome[i] -= password;
            }
        }

        public static void ConvertToColorMapping()
        {
            byte[] vec = (byte[])Chromosome.Clone();
            ColorChromosome = new Color[vec.Length / 3 + 1];
            byte r, g, b;
            for (int i = 0; i < (vec.Length) / 3 + 1; i++)
            {
                r = 0; g = 0; b = 0;
                if (3 * i < vec.Length)
                {
                    r = vec[3 * i];
                }
                if (3 * i + 1 < vec.Length)
                {
                    g = vec[3 * i + 1];
                }
                if (3 * i + 2 < vec.Length)
                {
                    b = vec[3 * i + 2];
                }
                ColorChromosome[i] = Color.FromArgb(r, g, b);
            }
        }

        public static Bitmap GetPicture()
        {
            byte[] pin = new byte[HEADERLEN];
            int blen = Chromosome.Length;
            int clen = ColorChromosome.Length;

            byte[] bytearray = BitConverter.GetBytes(blen);
            for (int i = 0; i < bytearray.Length; i++)
            {
                pin[i] = bytearray[i];
            }
            List<Color> clist = new List<Color>();
            for (int i = 0; i < HEADERCOLORLEN; i++)
            {
                clist.Add(Color.FromArgb(pin[3 * i], pin[3 * i + 1], pin[3 * i + 2]));
            }
            for (int i = 0; i < ColorChromosome.Length; i++)
            {
                clist.Add(ColorChromosome[i]);
            }

            double p = Math.Sqrt(clist.Count);
            int picsize = (int)p + 1;
            Bitmap ret = new Bitmap(picsize, picsize);

            int index = 0;
            bool check = false;
            for (int i = 0; i < picsize; i++)
            {
                for (int j = 0; j < picsize; j++)
                {
                    ret.SetPixel(i, j, clist[index++]);
                    if (index >= clist.Count)
                    {
                        check = true;
                        break;
                    }
                }
                if (check) { break; }
            }

            return ret;
        }

        public static bool ToMap(string target, string name, RemoveFlag Flag, byte password = 0, string comment = "")
        {
            bool ret = true;
            Location = target;
            CopyTarget(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            ArrangementTarget(Flag);
            Compress();
            ConvertToChromosome(password);
            ConvertToColorMapping();
            GetPicture().Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + name + ".bmp");

            File.Delete(TemporaryZipLocation);
            DeleteDirectory(Location);
            Location = string.Empty;
            return ret;
        }

        #endregion

        #region Convert To Box

        public static void ConvertToBox(string temporaryLocation)
        {
            using (FileStream fs = new FileStream(temporaryLocation, FileMode.Create, FileAccess.Write))
            {
                fs.Write(Chromosome, 0, Chromosome.Length);
            }
        }

        public static void GetChromosome(byte password = 0)
        {
            List<Color> clist = new List<Color>();
            using (Bitmap b = new Bitmap(Picture))
            {
                int picsize = b.Width;
                for (int i = 0; i < picsize; i++)
                {
                    for (int j = 0; j < picsize; j++)
                    {
                        clist.Add(b.GetPixel(i, j));
                    }
                }
            }

            byte[] header = new byte[HEADERLEN];
            for (int i = 0; i < HEADERCOLORLEN; i++)
            {
                header[3 * i] = clist[i].R;
                header[3 * i + 1] = clist[i].G;
                header[3 * i + 2] = clist[i].B;
            }
            for (int i = 0; i < HEADERCOLORLEN; i++)
            {
                clist.Remove(clist[0]);
            }

            long blen = BitConverter.ToInt64(header, 0);
            Chromosome = new byte[blen];
            int index = 0;
            for (int i = 0; i < clist.Count; i++)
            {
                Chromosome[index++] = clist[i].R; if (index >= blen) { break; }
                Chromosome[index++] = clist[i].G; if (index >= blen) { break; }
                Chromosome[index++] = clist[i].B; if (index >= blen) { break; }
            }
            for (int i = 0; i < Chromosome.Length; i++)
            {
                Chromosome[i] += password;
            }
        }

        public static bool ExtructZip(string path)
        {
            bool ret = true;
            try
            {

                if (System.IO.File.Exists(path))
                {
                    ZipFile.ExtractToDirectory(path, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Encoding.GetEncoding("shift_jis"));
                }
            }
            catch (Exception)
            {
                ret = false;
            }

            return ret;
        }

        public static bool ToBox(string path, byte password = 0)
        {
            bool ret = false;
            if (File.Exists(path))
            {
                Picture = path;
                Location = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                GetChromosome(password);
                ConvertToBox(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\La+.zip");
                if (ExtructZip(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\La+.zip"))
                {
                    ret = true;
                    File.Delete(path);
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\La+.zip");
                }
                else
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\La+.zip");
                }
            }
            return ret;
        }
        #endregion

        #region private method
        /// <summary>
        /// ディレクトリをコピーする
        /// </summary>
        /// <param name="sourceDirName">コピーするディレクトリ</param>
        /// <param name="destDirName">コピー先のディレクトリ</param>
        private static void CopyDirectory(
            string sourceDirName, string destDirName)
        {
            //コピー先のディレクトリがないときは作る
            if (!System.IO.Directory.Exists(destDirName))
            {
                System.IO.Directory.CreateDirectory(destDirName);
                //属性もコピー
                System.IO.File.SetAttributes(destDirName,
                    System.IO.File.GetAttributes(sourceDirName));
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] !=
                    System.IO.Path.DirectorySeparatorChar)
                destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            string[] files = System.IO.Directory.GetFiles(sourceDirName);
            foreach (string file in files)
                System.IO.File.Copy(file,
                    destDirName + System.IO.Path.GetFileName(file), true);

            //コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
            string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
                CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));
        }

        private static void DeleteTarget(RemodeConditionCheck checkF, DirectoryInfo dinfo)
        {
            List<string> RemoveTarget = new List<string>();
            foreach (var item in dinfo.GetFiles())
            {
                if (checkF(item.FullName, checkMode.File))
                {
                    RemoveTarget.Add(item.FullName);
                }
            }
            foreach (var item in RemoveTarget)
            {
                File.Delete(item);
            }

            RemoveTarget.Clear();
            foreach (var item in dinfo.GetDirectories())
            {
                if (checkF(item.FullName, checkMode.Directory))
                {
                    RemoveTarget.Add(item.FullName);
                }
            }
            foreach (var item in RemoveTarget)
            {
                DeleteDirectory(item);
            }

            foreach (var item in dinfo.GetDirectories())
            {
                DeleteTarget(checkF, item);
            }
        }

        private static void DeleteDirectory(string targetDirectoryPath)
        {
            if (!Directory.Exists(targetDirectoryPath))
            {
                return;
            }

            //ディレクトリ以外の全ファイルを削除
            string[] filePaths = Directory.GetFiles(targetDirectoryPath);
            foreach (string filePath in filePaths)
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            //ディレクトリの中のディレクトリも再帰的に削除
            string[] directoryPaths = Directory.GetDirectories(targetDirectoryPath);
            foreach (string directoryPath in directoryPaths)
            {
                DeleteDirectory(directoryPath);
            }

            //中が空になったらディレクトリ自身も削除
            Directory.Delete(targetDirectoryPath, false);
        }

        private static void GetFileList(DirectoryInfo dinfo, string remove, ref List<string> flist, ref List<string> dlist)
        {
            foreach (var item in dinfo.GetFiles())
            {
                flist.Add(item.FullName);
                dlist.Add(item.Directory.FullName.Replace(remove, ""));
            }
            foreach (var item in dinfo.GetDirectories())
            {
                GetFileList(item, remove, ref flist, ref dlist);
            }
        }

        #endregion
    }
}
