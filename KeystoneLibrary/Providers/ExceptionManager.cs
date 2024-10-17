using KeystoneLibrary.Interfaces;
using Microsoft.Data.SqlClient;

namespace KeystoneLibrary.Providers
{
    public class ExceptionManager : IExceptionManager
    {
        public bool IsDuplicatedEntityCode(Exception e)
        {
            if (e.InnerException is SqlException sqlException 
                && (sqlException.Number == 2601 || sqlException.Number == 2627))
            {
                return true;
            }

            return false;
        }
    }
}