using Abletech.Arxivar.Client;
using Abletech.Arxivar.Entities;
using Abletech.Arxivar.Entities.Enums;
using Abletech.Arxivar.Entities.Exceptions;
using Abletech.Arxivar.Entities.Libraries;
using Abletech.Arxivar.Entities.Search;
using Poker.Lib.Arxivar.Models;
using Poker.Lib.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Lib.Arxivar.Services
{
    public class ProfileService
    {
        protected WCFConnectorManager _manager;
        protected string documentType;

        public ProfileService()
        {
            _manager = WcfClient.Instance.ConnectionManager;
            documentType = Config.Instance.Retrieve("arxivar", "documentType");
        }

        public Dm_Profile_Result Create(string id, Contact contact, in byte[] pdf, in byte[] photo)
        {
            Dm_Profile_ForInsert insert = _manager.ARX_DATI.Dm_Profile_ForInsert_Get_New_Instance_ByDocumentTypeCodice(documentType);

            insert.Aoo = Config.Instance.Retrieve("arxivar", "aoo");
            insert.DocName = string.Format("Richiesta BIP # {0}", id);
            insert.InOut = DmProfileInOut.Entrata;
            insert.Stato = Config.Instance.Retrieve("arxivar", "documentState");
            insert.DataDoc = DateTime.Now.Date;
            insert.File = BuildArxFile(pdf, "modulo-richiesta.pdf");
            insert.Attachments.Add(BuildArxFile(photo, "fototessera.jpg"));

            // Fill contact
            ContactService c = new ContactService();
            // insert.From = c.GetDatiProfilo(contact.taxId, Dm_DatiProfilo_Campo.MI);

            // Fill in request ID field
            Aggiuntivo_String numero_richiesta = insert.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "NUMERO_RICHIESTA", StringComparison.CurrentCultureIgnoreCase)) as Aggiuntivo_String;
            numero_richiesta.Valore = id;

            // Hash and fill in sha256 field
            using (var sha256 = new System.Security.Cryptography.SHA256Managed())
            {
                Aggiuntivo_String hash = insert.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "SHA256_HASH", StringComparison.CurrentCultureIgnoreCase)) as Aggiuntivo_String;
                hash.Valore = BitConverter.ToString(sha256.ComputeHash(insert.File.ToMemoryStream())).Replace("-", "");
            }

            return _manager.ARX_DATI.Dm_Profile_Insert(insert);
        }

        public List<Profile> Select(string id)
        {
            using (Dm_Profile_Select profileSelect = _manager.ARX_SEARCH.Dm_Profile_Select_Get_New_Instance_By_TipiDocumentoCodice(documentType))
            using (Dm_Profile_Search profileSearch = _manager.ARX_SEARCH.Dm_Profile_Search_Get_New_Instance_By_TipiDocumentoCodice(documentType))
            {
                profileSelect.MaxRowCount = 10;
                profileSelect.DOCNUMBER.Selected = true;
                profileSelect.DOCNUMBER.Index = 0;
                profileSelect.DOCNAME.Selected = true;
                profileSelect.DOCNAME.Index = 1;
                profileSelect.ORIGINALE.Selected = true;
                profileSelect.ORIGINALE.Index = 2;
                profileSelect.CREATION_DATE.Selected = true;
                profileSelect.CREATION_DATE.Index = 3;
                profileSelect.FILENAME.Selected = true;
                profileSelect.FILENAME.Index = 4;

                // Add hash from custom fields selection
                if (!(profileSelect.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "SHA256_HASH", StringComparison.CurrentCultureIgnoreCase)) is Aggiuntivo_Selected hash))
                {
                    throw new Exception("SHA256_HASH field not found");
                }
                hash.Selected = true;
                hash.Index = 5;

                // Add request ID custom field filter
                if (!(profileSearch.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "NUMERO_RICHIESTA", StringComparison.CurrentCultureIgnoreCase)) is Field_String numero_richiesta))
                {
                    throw new Exception("NUMERO_RICHIESTA field not found");
                }
                numero_richiesta.SetFilter(Dm_Base_Search_Operatore_String.Uguale, id);
                numero_richiesta.forceCaseInsensitive = true;

                // Execute search and return selected data as a list
                using (var ds = _manager.ARX_SEARCH.Dm_Profile_GetData(profileSearch, profileSelect, 0))
                {
                    return ds.GetDataTable(0).Select().Select(e => new Profile(e.ItemArray)).ToList();
                }
            }
        }

        public System.IO.MemoryStream Download(int id)
        {
            var arxFile = _manager.ARX_DOCUMENTI.Dm_Profile_GetDocument(id);

            /*
            var folder = System.IO.Path.GetTempPath();
            var fileName = Guid.NewGuid().ToString("N").Substring(1, 6) + "_" + arxFile.FileName;
            arxFile.SaveTo(folder, fileName);
            return System.IO.Path.Combine(folder, fileName);
            */

            return arxFile.ToMemoryStream();
        }

        public bool Update(int id, in byte[] pdf, string name)
        {
            ArxGenericException age;

            // Attempt document update
            bool update = _manager.ARX_DOCUMENTI.Dm_Profile_SetDocument_Advanced(
                out age,
                BuildArxFile(pdf, name),
                id,
                string.Empty);

            return update;
        }

        public bool Destroy(string id)
        {
            var profile = Select(id);
            return _manager.ARX_DATI.Dm_Profile_Delete(profile[0].id) == 1;
        }

        public Arx_File BuildArxFile(in byte[] pdf, string name)
        {
            return new Arx_File(pdf, name, DateTime.Now.Date);
        }
    }
}
