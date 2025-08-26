using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KiCData.Models;
using KiCData.Models.WebModels;
using KiCData.Models.WebModels.Member;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Intrinsics.Arm;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace KiCData.Services
{
	public class UserService : IUserService
	{
		public WebUser CreateUser(RegisterViewModel rvm)
		{
			WebUser user = new WebUser(rvm);

			user.HashedPassword = EncryptPassword(user, rvm.Password);

			return user;
		}

		private string EncryptPassword(WebUser user, string password)
		{
			string salt = user.EmailAddress.Split('@')[1];

			string saltedPassword = password + salt;

			SHA256 hashAlgorithm = SHA256.Create();

			byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

			string result = Convert.ToBase64String(hash);

			return result;
		}
		
		public void GiveRoles(WebUser user, HttpContextAccessor httpContextAccessor)
		{
			AspNetUserManager<User> managedUser = new AspNetUserManager<User>();
			
			managedUser.
		}
		
		public bool HasPriv(HttpContextAccessor httpContextAccessor)
		{
			httpContextAccessor.HttpContext.User.IsInRole
			
			return false;
		}
	}
}