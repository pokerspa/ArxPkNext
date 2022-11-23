using Abletech.Arxivar.Client;
using Abletech.Arxivar.Entities;
using Abletech.Arxivar.Entities.Enums;
using Abletech.Arxivar.Entities.Libraries;
using Abletech.Arxivar.Entities.Search;
using Poker.Lib.FormData;
using Poker.Lib.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker.Lib.Arxivar.Services
{
    public class ContactService
    {
        protected WCFConnectorManager _manager;

        public ContactService()
        {
            _manager = WcfClient.Instance.ConnectionManager;
        }

        public Dm_CatRubriche GetAddressBook()
        {
            var rubriche = _manager.ARX_DATI.Dm_CatRubriche_Get_Data("");
            return rubriche.First(x => string.Equals(x.RUBRICA, Config.Instance.Retrieve("arxivar", "addressBook"), StringComparison.CurrentCultureIgnoreCase));
        }

        public Dm_DatiProfilo GetOrCreateLink(Dictionary<string, string> data, Dm_DatiProfilo_Campo type)
        {
            var contact = GetOrCreateEntry(data);
            return _manager.ARX_DATI.Dm_DatiProfilo_GetNewInstance_From_IdRubrica(contact.SYSTEM_ID, type);
        }

        public List<Dm_DatiProfilo> GetCopyLinks(Dictionary<string, string> data, Dm_DatiProfilo_Campo type)
        {
            List<Dm_DatiProfilo> result = new List<Dm_DatiProfilo> { };

            using (var search = new Dm_Contatti_Search())
            using (var select = new Dm_Contatti_Select())
            {
                select.DM_RUBRICA_RAGIONE_SOCIALE.Selected = true;
                select.DM_RUBRICA_SYSTEM_ID.Selected = true;
                select.CONTATTO.Selected = true;
                select.ID.Selected = true;

                search.Dm_Rubrica.CODFIS.SetFilter(Dm_Base_Search_Operatore_String.Uguale, data["tax_id"]);
                search.Dm_Rubrica.CODFIS.forceCaseInsensitive = true;

                search.CONTATTO.SetFilter(Dm_Base_Search_Operatore_String.Nullo_o_Vuoto, "");

                using (var ds = _manager.ARX_SEARCH.Dm_Contatti_GetData(search, select))
                {
                    var dt = ds.GetDataTable(0);

                    // Return prematurely if there are no results
                    if (dt.Rows.Count == 0) return result;

                    foreach (var element in dt.Select())
                    {
                        int id = Decimal.ToInt32((decimal)element["ID"]);
                        var contact = _manager.ARX_DATI.Dm_DatiProfilo_GetNewInstance_From_IdContatto(id, type);
                        result.Add(contact);
                    }
                }
            }

            return result;
        }

        public Dm_Rubrica GetOrCreateEntry(Dictionary<string, string> data)
        {
            using (var search = new Dm_Contatti_Search())
            using (var select = new Dm_Contatti_Select())
            {
                select.DM_RUBRICA_RAGIONE_SOCIALE.Selected = true;
                select.DM_RUBRICA_SYSTEM_ID.Selected = true;
                select.CONTATTO.Selected = true;
                select.ID.Selected = true;

                search.Dm_Rubrica.CODFIS.SetFilter(Dm_Base_Search_Operatore_String.Uguale, data["tax_id"]);
                search.Dm_Rubrica.CODFIS.forceCaseInsensitive = true;

                search.CONTATTO.SetFilter(Dm_Base_Search_Operatore_String.Nullo_o_Vuoto, "");

                using (var ds = _manager.ARX_SEARCH.Dm_Contatti_GetData(search, select))
                {
                    var dt = ds.GetDataTable(0);

                    // Create new contact if there are no results
                    if (dt.Rows.Count == 0) return CreateContact(data);

                    // Fetch existing contact by System ID when found
                    int id = Decimal.ToInt32((decimal)dt.Rows[0]["DM_RUBRICA_SYSTEM_ID"]);
                    return _manager.ARX_DATI.Dm_Rubrica_Get_DataBySystemId(id);
                }
            }
        }


        public Dm_Rubrica CreateContact(Dictionary<string, string> data)
        {
            var rubrica = GetAddressBook();

            Dm_Rubrica_ForInsert insert = _manager.ARX_DATI.Dm_Rubrica_ForInsert_GetNewInstance_By_DmCatRubricheId(rubrica.ID);
            insert.AOO = Config.Instance.Retrieve("arxivar", "aoo");
            insert.TIPO = Dm_Rubrica_Tipo.Ditta;
            insert.RAGIONE_SOCIALE = string.Format("{0} {1}", data["first_name"], data["last_name"]);
            insert.MAIL = data["email"];
            insert.TEL = data["phone"];
            insert.STATO = Dm_Rubrica_Stato.Active;
            insert.CODFIS = data["tax_id"];

            insert.INDIRIZZO = data["address"];
            insert.LOCALITA = data["address_city"];
            insert.CAP = data["address_zip"];
            insert.PROVINCIA = data["address_province"];

            using (Aggiuntivo_String field = insert.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "Genere", StringComparison.CurrentCultureIgnoreCase)) as Aggiuntivo_String)
            {
                field.Valore = data["gender"];
                insert.Aggiuntivi.Add(field);
            }

            using (Aggiuntivo_String field = insert.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "Luogo di Nascita", StringComparison.CurrentCultureIgnoreCase)) as Aggiuntivo_String)
            {
                field.Valore = string.Format("{0} ({1}) {2}", data["birth_place_city"], data["birth_place_province"], data["birth_place_country"]);
                insert.Aggiuntivi.Add(field);
            }

            using (Aggiuntivo_String field = insert.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "Fruitore della Detrazione", StringComparison.CurrentCultureIgnoreCase)) as Aggiuntivo_String)
            {
                field.Valore = data["payer_tax_id"];
                insert.Aggiuntivi.Add(field);
            }

            using (Aggiuntivo_String field = insert.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "Indirizzo Richiedente", StringComparison.CurrentCultureIgnoreCase)) as Aggiuntivo_String)
            {
                field.Valore = string.Format("{0} {1} {2} ({3})", data["address"], data["address_zip"], data["address_city"], data["address_province"]);
                insert.Aggiuntivi.Add(field);
            }

            using (Aggiuntivo_String field = insert.Aggiuntivi.FirstOrDefault(x => string.Equals(x.ExternalId, "Indirizzo Genitore/Tutore", StringComparison.CurrentCultureIgnoreCase)) as Aggiuntivo_String)
            {
                if (data.ContainsKey("guardian_address") && !String.IsNullOrEmpty(data["guardian_address"]))
                {
                    field.Valore = string.Format("{0} {1} {2} ({3})", data["guardian_address"], data["guardian_address_zip"], data["guardian_address_city"], data["guardian_address_province"]);
                    insert.Aggiuntivi.Add(field);
                }
            }

            Dm_Contatti_ForInsert owner = BipFormDataContact.FillFromFormData(data, "", "Richiedente");
            insert.Contatti.Add(owner);

            if (data.ContainsKey("guardian_first_name"))
            {
                Dm_Contatti_ForInsert guardian = BipFormDataContact.FillFromFormData(data, "guardian_", "Genitore/Tutore");
                insert.Contatti.Add(guardian);
            }

            return _manager.ARX_DATI.Dm_Rubrica_Insert(insert);
        }
    }
}
