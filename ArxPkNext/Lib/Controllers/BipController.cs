using Abletech.Arxivar.Client;
using Abletech.Arxivar.Entities.Enums;
using Abletech.Arxivar.Entities.Libraries;
using Poker.Lib.Arxivar.Models;
using Poker.Lib.Arxivar.Services;
using Poker.Lib.Http;
using Poker.Lib.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Poker.Lib.controllers
{
    public class BipController
    {
        public static void Create(HttpListenerContext context, GroupCollection RouteVars, MultipartParser multipart)
        {
            Response res = new Response(ref context);
            try
            {
                string id = multipart.fields["id"];
                byte[] pdf = multipart.fields["submission_form"].Base64ToByteArray();
                byte[] photo = multipart.fields["passport_photo"].Base64ToByteArray();

                using (WCFConnectorManager wcf = WcfClient.Instance.ConnectionManager)
                {
                    ProfileService profili = new ProfileService();

                    try
                    {
                        Dm_Profile_Result record = profili.Create(id, multipart.fields, pdf, photo);

                        if (record.EXCEPTION == Security_Exception.Nothing)
                        {
                            res.SetResponse("Successfully created document.");
                            res.SetStatus(true);
                            res.Send(HttpStatusCode.OK);
                        }
                        else
                        {
                            res.SetResponse(record.MESSAGE);
                            res.SetStatus(false);
                            res.Send(HttpStatusCode.InternalServerError);
                        }
                    }
                    catch (Exception e)
                    {
                        res.SetResponse("mi sono rotto nell'using " + e.ToString());
                        res.SetStatus(false);
                        res.Send(HttpStatusCode.InternalServerError);
                    }
                }

            }
            catch (Exception e)
            {
                res.SetResponse("mi sono rotto nei primi 3 " + e.ToString());
                res.SetStatus(false);
                res.Send(HttpStatusCode.InternalServerError);
            }
        }

        public static void List(HttpListenerContext context, GroupCollection RouteVars, MultipartParser multipart)
        {
            Response res = new Response(ref context);

            using (WCFConnectorManager wcf = WcfClient.Instance.ConnectionManager)
            {
                ProfileService profili = new ProfileService();

                List<Profile> files = profili.Select(RouteVars["SYSTEMID"].Value);

                if (files.Count > 0)
                {
                    Dictionary<string, object> d = new Dictionary<string, object>();

                    // Add file list to response
                    d.Add("files", files);

                    // Add hook injection to handle post-sign events
                    d.Add("script", "hook.js");

                    res.SetResponse(d);
                    res.SendPureResponse();
                    res.SetStatus(true);
                    res.Send(HttpStatusCode.OK);
                }
                else
                {
                    res.SetResponse("No files found for requested ID");
                    res.SetStatus(false);
                    res.Send(HttpStatusCode.NotFound);
                }
            }
        }

        public static void Download(HttpListenerContext context, GroupCollection RouteVars, MultipartParser multipart)
        {
            string sysid = RouteVars["SYSTEMID"].Value;
            int docid = System.Convert.ToInt32(RouteVars["DOCID"].Value);
            Response res = new Response(ref context);

            using (WCFConnectorManager wcf = WcfClient.Instance.ConnectionManager)
            {
                ProfileService profili = new ProfileService();
                var files = profili.Select(sysid);

                if (files.Count > 0)
                {
                    MemoryStream stream = profili.Download(files[docid].id);
                    res.SetResponse(stream);
                    res.SendAsOctet();
                    res.SetContentType("application/pdf");
                    res.SetStatus(true);
                    res.Send(HttpStatusCode.OK);
                }
                else
                {
                    res.SetResponse("No files found for requested ID");
                    res.SetStatus(false);
                    res.Send(HttpStatusCode.NotFound);
                }
            }
        }

        public static void Destroy(HttpListenerContext context, GroupCollection RouteVars, MultipartParser multipart)
        {
            string sysid = RouteVars["SYSTEMID"].Value;
            Response res = new Response(ref context);
            ProfileService profili = new ProfileService();

            if (profili.Destroy(sysid))
            {
                res.SetResponse("Document deleted successfully!");
                res.SetStatus(true);
                res.Send(HttpStatusCode.OK);
            }
            else
            {
                res.SetResponse("Could not update document.");
                res.SetStatus(false);
                res.Send(HttpStatusCode.InternalServerError);
            }
        }

        public static void Seal(HttpListenerContext context, GroupCollection RouteVars, MultipartParser multipart)
        {
            string sysid = RouteVars["SYSTEMID"].Value;
            Response res = new Response(ref context);
            byte[] pdf = multipart.fields["submission_form"].Base64ToByteArray();
            string hash = multipart.fields["hash"];

            using (WCFConnectorManager wcf = WcfClient.Instance.ConnectionManager)
            {
                ProfileService profili = new ProfileService();
                Profile doc = profili.Select(sysid)[0];
                bool update = profili.Update(doc.id, pdf, "richiesta.pdf", hash);

                if (update)
                {
                    res.SetResponse("Document updated successfully!");
                    res.SetStatus(true);
                    res.Send(HttpStatusCode.OK);
                }
                else
                {
                    res.SetResponse("Could not update document.");
                    res.SetStatus(false);
                    res.Send(HttpStatusCode.InternalServerError);
                }
            }
        }
    }
}
