using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Win32;
using File = Google.Apis.Drive.v3.Data.File;
using System.Text;

namespace BenchmarkingCloudStorage
{
    class Program
    {
        // If you modify these scopes, delete your previously saved credentials at ~/.credentials/drive-dotnet-quickstart.json
        // Or it won't work :)
        static string[] Scopes = { DriveService.Scope.Drive,
                           DriveService.Scope.DriveAppdata,
                           DriveService.Scope.DriveFile,
                           DriveService.Scope.DrivePhotosReadonly,
                           DriveService.Scope.DriveMetadataReadonly,
                           DriveService.Scope.DriveReadonly,
                           DriveService.Scope.DriveScripts };
        static string ApplicationName = "Benchmarking Cloud Storage";

        static void Main(string[] args)
        {
            // Create user credentials.
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            String ime = "poskus";
            int num_files = 10;

            TimeSpan t_sum = TimeSpan.Zero;
            GeneriranjeBremena(ime,num_files, 100, 1);
       
            for(int i = 0; i < num_files; i++)
            {

                string filePath = "../../files/"+ime+i+".txt";
                DateTime t1 = DateTime.Now;
                UploadFile(service, filePath);
                TimeSpan t = DateTime.Now - t1;
                Console.WriteLine(t);
                t_sum += t;
            }
            Console.WriteLine(t_sum);
            ListFiles(service);
        }

        // List all files on Drive.
        private static void ListFiles(DriveService _service)
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            
            IList<File> files = listRequest.Execute().Files;

            Console.WriteLine("Files:");

            if (files != null && files.Count > 0)
            {
                foreach (var f in files)
                {
                    Console.WriteLine("{0} ({1})", f.Name, f.Id);
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            Console.Read();
        }

        // Get the mime type of the file. 
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(ext);

            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();

            return mimeType;
        }
        
        // Upload file to Drive.
        public static File UploadFile(DriveService _service, string _uploadFile)
        {
            if (System.IO.File.Exists(_uploadFile))
            {
                File body = new File();
                body.Name = Path.GetFileName(_uploadFile);
                body.Description = "Test upload";
                body.MimeType = GetMimeType(_uploadFile);
                
                // File's content. 
                byte[] byteArray = System.IO.File.ReadAllBytes(_uploadFile);
                MemoryStream stream = new MemoryStream(byteArray);

                try
                {
                    FilesResource.CreateMediaUpload request = _service.Files.Create(body, stream, GetMimeType(_uploadFile));
                    request.Upload();
                    return request.ResponseBody;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            Console.WriteLine("File does not exist: " + _uploadFile);
            return null;
        }
        /**
         * Funkcija za generiranje bremena glede na podane argumente
         * fileNum - stevilo datotek, ki jih zelimo generirati
         * fileSize - velikost datotek, ki jih zelimo generirati
         * sizeType - dolocitev velikosti (KB, MB, GB)
         */
        static void GeneriranjeBremena(string fileName , int fileNum, int fileSize, int sizeType)
        {

            //string path = "../../files/poskus.txt";
            string path = "../../files/";
            string pathO = "../../files/";
            string randomS = "";

            int sizeTransform = 0;
            //int upperLimit = 0;
            //upperLimit = 2 * (int)(Math.Pow(10, 6));
            //int loopNum = 1;

            if (sizeType == 1)
                sizeTransform = fileSize * (int)Math.Pow(10, 3);
            else if (sizeType == 2)
                sizeTransform = fileSize * (int)Math.Pow(10, 6);
            else if (sizeType == 3)
                sizeTransform = fileSize * (int)Math.Pow(10, 9);

            //if(sizeTransform >= upperLimit)
            //{
            //    loopNum = 2;
            //}


            /**
             * Zanka za generiranje zahtevanega števila datotek
             */
            for (int x = 0; x < fileNum; x++)
            {
                path = pathO;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append(path).Append(fileName).Append(x).Append(".txt");
                //Console.WriteLine(sb);
                path = sb.ToString();
                Console.WriteLine(path);
                try
                {
                    // Delete the file if it exists.
                    if (System.IO.File.Exists(path))
                    {
                        // Note that no lock is put on the
                        // file and the possibility exists
                        // that another process could do
                        // something with it between
                        // the calls to Exists and Delete.
                        System.IO.File.Delete(path);
                    }

                    // Create the file. 
                    //Sprazni vsebino datoteke ce ze obstaja
                    using (FileStream fs = System.IO.File.Create(path))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes("");
                        fs.Write(info, 0, info.Length);
                        fs.Close();
                    }

                    //dodaja posamezne crke v datoteko dokler ni pravilne velikosti
                    using (StreamWriter sw = System.IO.File.AppendText(path))
                    {
                        for (int i = 0; i < sizeTransform; i++)
                        {
                            randomS = RandomString(1);
                            //Byte[] info = new UTF8Encoding(true).GetBytes(randomS);
                            // Add some information to the file.
                            sw.Write(randomS);
                        }

                        sw.Close();
                    }

                    // Open the stream and read it back.
                    //using (StreamReader sr = File.OpenText(path))
                    //{
                    //    string s = "";
                    //    while ((s = sr.ReadLine()) != null)
                    //    {
                    //        Console.WriteLine(s);
                    //    }
                    //}
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                //string randomS = RandomString(fileSize);
                //System.Text.ASCIIEncoding.Unicode.GetByteCount(randomS);
                //System.Text.ASCIIEncoding.ASCII.GetByteCount(randomS);
                //Console.WriteLine(randomS);
            }

        }

        /**
         * Funkcija za generiranje naključnega Stringa
         */
        private static Random random = new Random();
        private static string RandomString(int Size)
        {
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < Size; i++)
            {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}