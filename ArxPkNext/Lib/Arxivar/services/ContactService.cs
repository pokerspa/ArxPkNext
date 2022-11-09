using Abletech.Arxivar.Client;
using Abletech.Arxivar.Entities;
using Abletech.Arxivar.Entities.Enums;
using Abletech.Arxivar.Entities.Libraries;
using Abletech.Arxivar.Entities.Search;
using Poker.Lib.Arxivar.Models;
using Poker.Lib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poker.Lib.Arxivar.Services
{
    public class ContactService
    {
        protected WCFConnectorManager _manager;

        public ContactService()
        {
            _manager = WcfClient.Instance.ConnectionManager;
        }

        public int? CercaInRubrica(string codiceFiscale)
        {
            using (var select = new Dm_Contatti_Select())
            using (var search = new Dm_Contatti_Search())
            {
                select.DM_RUBRICA_RAGIONE_SOCIALE.Selected = true;
                select.DM_RUBRICA_SYSTEM_ID.Selected = true;
                select.CONTATTO.Selected = true;
                select.ID.Selected = true;

                search.Dm_Rubrica.CODFIS.SetFilter(Dm_Base_Search_Operatore_String.Uguale, codiceFiscale);
                search.Dm_Rubrica.CODFIS.forceCaseInsensitive = true;
                /*search.Dm_Rubrica.PARTIVA.SetFilter(Dm_Base_Search_Operatore_String.Uguale, codiceFiscale);
                search.Dm_Rubrica.PARTIVA.forceCaseInsensitive = true;*/

                search.CONTATTO.SetFilter(Dm_Base_Search_Operatore_String.Nullo_o_Vuoto, "");

                using (var ds = _manager.ARX_SEARCH.Dm_Contatti_GetData(search, select))
                {
                    var dt = ds.GetDataTable(0);

                    if (dt.Rows.Count == 0)
                    {
                        return null;
                    }

                    var resultId = System.Convert.ToInt32(dt.Rows[0]["ID"]);
                    return resultId;
                }
            }
        }

        public Dm_CatRubriche GetRubrica()
        {
            var rubriche = _manager.ARX_DATI.Dm_CatRubriche_Get_Data("");
            return rubriche.First(x => string.Equals(x.RUBRICA, Config.Instance.Retrieve("arxivar", "addressBook"), StringComparison.CurrentCultureIgnoreCase));
        }

        public Dm_Rubrica InsertRubrica(Contact contact)
        {
            var rubrica = GetRubrica();

            var insert = _manager.ARX_DATI.Dm_Rubrica_ForInsert_GetNewInstance_By_DmCatRubricheId(rubrica.ID);
            insert.RAGIONE_SOCIALE = contact.nameId;
            insert.MAIL = contact.email;
            insert.TEL = contact.phone;
            insert.CODFIS = contact.taxId;
            insert.CELL = contact.phone;

            var rc = new Dm_Contatti_ForInsert
            {
                CONTATTO = contact.fullName,
                EMAIL = contact.email,
                CELL = contact.phone,
            };
            insert.Contatti.Add(rc);

            Dm_Rubrica result = _manager.ARX_DATI.Dm_Rubrica_Insert(insert);
            return result;
        }

        public Dm_DatiProfilo GetDatiProfilo(string codFisc, Dm_DatiProfilo_Campo tipoCampo)
        {
            //Cerchiamo la persona nella rubrica...
            var idContatto = CercaInRubrica(codFisc);

            //Il contatto è nella rubrica?
            if (idContatto != null)
            {
                return _manager.ARX_DATI.Dm_DatiProfilo_GetNewInstance_From_IdContatto(idContatto.Value, tipoCampo);
            }
            else
            {
                return _manager.ARX_DATI.Dm_DatiProfilo_Insert(new Dm_DatiProfilo
                {
                    VALORE = codFisc,
                    CODFIS = codFisc,
                    CAMPO = tipoCampo
                });
            }
        }
    }
}
