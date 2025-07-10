using Dapper;
using System;
using System.Data;
using System.Globalization;
using System.Linq;

public class PgvectorTypeHandler : SqlMapper.TypeHandler<float[]>
{
    public override float[] Parse(object value)
    {
        if (value is string str)
        {
            str = str.Trim('[', ']');
            return str.Split(',').Select(s => float.Parse(s.Trim(), CultureInfo.InvariantCulture)).ToArray();
        }

        throw new InvalidCastException($"Cannot cast {value.GetType()} to float[]");
    }

    public override void SetValue(IDbDataParameter parameter, float[] value)
    {
        parameter.Value = $"[{string.Join(",", value.Select(v => v.ToString(CultureInfo.InvariantCulture)))}]";
        parameter.DbType = DbType.String;
    }
}