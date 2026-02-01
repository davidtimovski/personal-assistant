using System.Data;
using Dapper;

namespace Jobs.Dapper;

/// <summary>
/// Necessary due to Dapper not supporting <see cref="DateOnly" /> yet.
/// </summary>
public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => (DateOnly)value;

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }
}
