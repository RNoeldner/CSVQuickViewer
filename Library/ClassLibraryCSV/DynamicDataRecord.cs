
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace CsvTools
{
  public class DynamicDataRecord : DynamicObject
  {
    private readonly Dictionary<string, object> m_Properties;

    public DynamicDataRecord(IDataRecord dataRecord)
    {
      m_Properties = new Dictionary<string, object>(dataRecord?.FieldCount ??  throw new ArgumentNullException(nameof(dataRecord)));
      for (var i = 0; i < dataRecord.FieldCount; i++)
        m_Properties.Add(dataRecord.GetName(i), dataRecord.GetValue(i));
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result) => m_Properties.TryGetValue(binder.Name, out result);

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
      dynamic method = m_Properties[binder.Name];
      result = method(args[0].ToString(), args[1].ToString());
      return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      try
      {
        m_Properties[binder.Name] = value;
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}