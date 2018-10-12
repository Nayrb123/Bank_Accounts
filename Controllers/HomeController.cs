using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using bank_accounts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace bank_accounts.Controllers
{
    public class HomeController : Controller
    {
        private YourContext dbContext;
        // here we can "inject" our context service into the constructor
        public HomeController(YourContext context)
        {
            dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View("Login");
        }

        [HttpGet("logout")]
        public IActionResult logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet("account/{id}")]
        public IActionResult showAccount(int id)
        {
            if (HttpContext.Session.GetInt32("logged_in_userID") == null)
            {
                return RedirectToAction("Login");
            }

            List<Transaction> alltransactions = dbContext.Transaction.Include(user => user.Handler).ToList();
            
            int logged_in_user = (int)HttpContext.Session.GetInt32("logged_in_userID");
            var current_user = dbContext.User.Include(user => user.Transactions).FirstOrDefault(user => user.Userid == logged_in_user);

            var transactions = current_user.Transactions;
            transactions.Reverse();
            decimal sum = 0;

            foreach(var i in transactions)
            {
                sum += i.Amount;
            }
            
            ViewBag.Balance = sum;

            
            ViewBag.AllTransactions = transactions;

            return View("AccountPage");
        }

        [HttpPost("transaction")]
        public IActionResult transaction(Transaction transaction)
        {
            int? logged_in_user = HttpContext.Session.GetInt32("logged_in_userID");

            transaction.Userid = (int)HttpContext.Session.GetInt32("logged_in_userID");
            //Making any negative amounts we use positive
            decimal relativeAmount = transaction.Amount * -1;
            //Grabbing logged in users id
            int logged_in_userid = (int)HttpContext.Session.GetInt32("logged_in_userID");

            var current_user = dbContext.User.Include(user => user.Transactions).FirstOrDefault(user => user.Userid == logged_in_userid);
            //Only transactions from the current logged in user
            var transactions = current_user.Transactions;

            decimal sum = 0;

            foreach(var i in transactions)
            {
                sum += i.Amount;
            }

            // create a variable with the balance to replace viewbag - compare with the relative amount
            if(relativeAmount > sum)
            {
                System.Console.WriteLine("triggered if");
                TempData["ErrorMessage"] = "Can not withdraw more than your current balance.";
                return Redirect($"account/{logged_in_user}");
            }
            else
            {
                System.Console.WriteLine("Triggered else");
            }
            dbContext.Add(transaction);
            dbContext.SaveChanges();
           
            return Redirect($"account/{logged_in_user}");
        }

        [HttpPost("login")]
        public IActionResult login(LoginUser user)
        {
            if(ModelState.IsValid)
            {
                var userindb = dbContext.User.FirstOrDefault(u => u.Email == user.Email);

                if(userindb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(user, userindb.Password, user.Password);

                if(result == 0)
                {
                    ModelState.AddModelError("Password", "Invalid Email/Password");
                    return View("Login");
                }
            
                // int logged_in_user_id = (int)userindb.id;
                HttpContext.Session.SetInt32("logged_in_userID", userindb.Userid); //Store logged in User's ID
                HttpContext.Session.SetString("logged_in_username", userindb.FirstName); //Store logged in Users First name
                
                int? logged_in_user = HttpContext.Session.GetInt32("logged_in_userID");
                System.Console.WriteLine(logged_in_user);

            return Redirect($"account/{logged_in_user}");
            }
            else
            {
                return View("Login");
            }
        }

        [HttpPost("register")]
        public IActionResult register(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.User.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }

                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                User Newuser = new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Password = user.Password,
                };

                dbContext.Add(Newuser);
                dbContext.SaveChanges();

                HttpContext.Session.SetInt32("logged_in_userID", Newuser.Userid); //Store logged in User's ID
                HttpContext.Session.SetString("logged_in_username", Newuser.FirstName); //Store logged in Users First name
                
                int? logged_in_user = HttpContext.Session.GetInt32("logged_in_userID");
                return Redirect($"account/{logged_in_user}");
            }
            else
            {
                return View("Index");
            }
        }
    }
}
