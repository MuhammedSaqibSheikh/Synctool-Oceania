using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRIM.SDK;

namespace EYLocationSyncUtility.BEL
{
    public class BaseRecord
    {
        public Record Record { get; set; }
        public bool NewRecord { get; set; }

        public Location GetClient(string idNumber)
        {
            Location result = null;

            var search = new TrimMainObjectSearch(Record.Database, BaseObjectTypes.Location);
            search.SetSearchString("id:" + idNumber);

            foreach (Location location in search)
            {
                result = location;
                break;
            }

            return result;
        }

        public void SetTitle(string title)
        {
            if (NewRecord || Record.Title != title)
            {
                Record.Title = title;
            }
        }

        public void SetRecordNumber(string number)
        {
            if (NewRecord || Record.Number != number)
            {
                Record.LongNumber = number;
            }
        }

        public void SetClient(Location client)
        {
            if (NewRecord || Record.Client.Uri != client.Uri)
            {
                Record.Client = client;
            }
        }

        public void SetStringField(string fieldName, string fieldValue)
        {

            if (NewRecord || Record.GetFieldValue(new FieldDefinition(Record.Database, fieldName)).ToString() != fieldValue)
            {
                Record.SetFieldValue(new FieldDefinition(Record.Database, fieldName), new UserFieldValue(fieldValue));
            }

        }

        public void SetDateTimeField(string fieldName, DateTime fieldValue)
        {
            if (NewRecord || Record.GetFieldValue(new FieldDefinition(Record.Database, fieldName)).AsDate().ToDateTime() != fieldValue)
            {
                Record.SetFieldValue(new FieldDefinition(Record.Database, fieldName), new UserFieldValue(fieldValue));
            }
        }

        public void SetLocationField(string fieldName, Location location)
        {
            
            var userFieldValue = Record.GetFieldValue(new FieldDefinition(Record.Database, fieldName));

            Location oldValue = userFieldValue == null ? null : userFieldValue.AsTrimObject() as Location;

            if ((NewRecord && location != null) ||
                (oldValue != null && location == null) ||
                (oldValue == null && location != null) ||
                (oldValue != null && location != null && oldValue.Uri != location.Uri))
            {
                Record.SetFieldValue(new FieldDefinition(Record.Database, fieldName), new UserFieldValue(location));
            }

        }

        public void SetInactiveDateField(DateTime inactiveDate)
        {
            if (NewRecord || Record.DateInactive.ToDateTime() != inactiveDate)
            {
                Record.DateInactive = inactiveDate;
            }

        }
    }
}
