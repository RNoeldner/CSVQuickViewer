using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace CsvTools
{
  public class DynamicDataRecord : DynamicObject
  {
    private readonly Dictionary<string, object> properties = null;

    public DynamicDataRecord(IDataRecord dataRecord)
    {
      properties = new Dictionary<string, object>(dataRecord?.FieldCount ??  throw new ArgumentNullException(nameof(dataRecord)));
      for (var i = 0; i < dataRecord.FieldCount; i++)
        properties.Add(dataRecord.GetName(i), dataRecord.GetValue(i));
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result) => properties.TryGetValue(binder.Name, out result);

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
      dynamic method = properties[binder.Name];
      result = method(args[0].ToString(), args[1].ToString());
      return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      try
      {
        properties[binder.Name] = value;
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}