using BackEndServer.Models.DBModels;
using BackEndServer.Models.ViewModels;
using BackEndServer.Services.AbstractServices;

namespace BackEndServer.Services
{
    public class UserService : AbstractUserService
    {
        private readonly IDatabaseQueryService _dbQueryService;
        
        public UserService(IDatabaseQueryService dbQueryService)
        {
            _dbQueryService = dbQueryService;
        }

        public UserSettings GetUserSettings(int userId)
        {
            DatabaseUser databaseUser = _dbQueryService.GetUserById(userId);
            return new UserSettings(databaseUser);
        }

        public bool ModifyUser(UserSettings userSettings)
        {
            return _dbQueryService.PersistExistingUser(new DatabaseUser(userSettings));
        }
    }
}