//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using AutoMapper;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Version4AutoReg.Models;
//using Version4AutoReg.Repositories;
//using Version4AutoRegAPI.Helpers;
//using Version4AutoRegAPI.Models;
//using Version4AutoRegAPI.Repositories;
//using Version4AutoRegAPI.Repositories.Interface;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using RestSharp;
//using Functions = Version4AutoReg.Helpers.Functions;

//namespace Version4AutoRegAPI.Controllers
//{
//    [ApiExplorerSettings(IgnoreApi = true)]
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PaymentController : Controller
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly AppSetup _appSetup;
//        private IAPIRepository _apiRepository;
//        string generalNotification = "General/";


//        public PaymentController(
//            UnitOfWork unitOfWork,
//            APIRepository repository,
//             Microsoft.Extensions.Options.IOptions<AppSetup> appSetup)
//        {
//            _unitOfWork = unitOfWork;
//            _apiRepository = repository;
//            _appSetup = appSetup.Value;
//        }

//        #region "interswitch"
//        ///// <summary>
//        /////     for state payment notifications
//        ///// </summary>
//        ///// <param name="model"></param>
//        ///// <returns></returns>
//        ///// <remark>
//        ///// sample response: {StatusCode="0",Message="Payment saved successfully",Reference= model.InvoiceReference}
//        ///// </remark>
//        [ProducesResponseType(200, Type = typeof(ISW_BookingResponse))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("CreateBooking")]
//        public async Task<ActionResult> CreateBooking(string ReferenceNumber, string Description, double Amount,
//            string FirstName, string LastName, string ItemCode, string Email = "")
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }
//                //check amount for equality
//                var newBooking = new ISW_BookingRequest();
//                string _responseFromInterswitch = string.Empty;
//                string MerchantId = "5052";
//                string SupportEmail = "support@courtevillegroup.com";

//                LastName = LastName?.Replace("&", " ");
//                FirstName = FirstName?.Replace("&", " ");
//                newBooking.MerchantId = MerchantId;
//                newBooking.ReferenceNumber = /*MerchantId +*/ ReferenceNumber;
//                newBooking.ItemCode = ItemCode;
//                newBooking.Email = SupportEmail;
//                newBooking.Description = Description;
//                newBooking.Amount = Amount * 100;//'Interswitch expects a multiple of hundred for the actual value;
//                newBooking.DateBooked = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
//                newBooking.DateExpired = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd hh:mm:ss");
//                newBooking.FirstName = FirstName?.Trim() == "" ? "NA" : FirstName;
//                newBooking.FirstName = (newBooking.FirstName == "-" || string.IsNullOrEmpty(newBooking.FirstName)) ? "NA" : newBooking.FirstName;
//                newBooking.LastName = (LastName?.Trim() == "" || string.IsNullOrEmpty(newBooking.LastName)) ? "NA" : LastName;
//                newBooking.LastName = newBooking.LastName == "-" ? "NA" : newBooking.LastName;

//                _responseFromInterswitch = await _apiRepository.PostInterswitchRequest(this.ModelInterswitchSoapRequest(newBooking));

//                return Ok(General.DECODE_Interswitch_Create_Booking_Response(_responseFromInterswitch));
//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, "An error has occured! Please try again");
//            }

//        }

//        #endregion
//        public static string GenerateSHA512String(string hlk)
//        {
//            SHA512 sha512 = SHA512Managed.Create();
//            byte[] bytes = Encoding.UTF8.GetBytes(hlk);
//            byte[] hash = sha512.ComputeHash(bytes);

//            StringBuilder result = new StringBuilder();
//            for (int i = 0; i <= hash.Length - 1; i++)
//                result.Append(hash[i].ToString("X2"));

//            return result.ToString();
//            //return GetStringFromHash(hash);
//        }

//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpPost, Route("PostNotification")]
//        public async Task<ActionResult> ABCNotificationAPI(DateTime from, DateTime to)
//        {
//            try
//            {
//                string res = "failure";
//                PaymentNotificationABC notABC = new PaymentNotificationABC();
//                var dreflist = GeneralSqlClient.GetRefList(from, to);
//                for (int i = 0; i < dreflist.Result.Count; i++)
//                {
//                    // var sysAPI = _unitOfWork.Invoices.GetInvoiceWithInvoiceUser    (Ref); // .VerifyInvoice(Ref);
//                    var sysAPI = GeneralSqlClient.GetLagosNotifications(dreflist.Result[i].Referenceno);
//                    if (sysAPI.Result.Count > 0)
//                    {
//                        notABC.CbnCode = null;
//                        // notABC.ClientId = "517578446363033";
//                        notABC.ClientId = "288539206722584";
//                        notABC.Webguid = sysAPI.Result[0].AlternativeReferenceno;
//                        notABC.AmountPaid = sysAPI.Result[0].Amount.ToString();
//                        //  notABC.State = "XXSG";
//                        notABC.State = "LASG";
//                        notABC.CreditAccount = "0004650857";
//                        notABC.BankNote = "Vehicle_Licnese_" + sysAPI.Result[0].Referenceno;
//                        notABC.PosId = null;
//                        notABC.TransId = null;
//                        //notABC.Hash = GenerateSHA512String("PH5UVZG993ND6BMADS65" + notABC.Webguid + notABC.State + notABC.AmountPaid);
//                        notABC.Hash = GenerateSHA512String("GE620HIT5B3F0M7C5LOE" + notABC.Webguid + notABC.State + notABC.AmountPaid);
//                        notABC.PaymentChannel = "EgolePay";
//                        notABC.PaymentRef = sysAPI.Result[0].Referenceno.ToString() + "1";
//                        notABC.TerminalId = null;
//                        notABC.TransRef = null;
//                        notABC.SendBy = null;
//                        notABC.Date = sysAPI.Result[0].Date.ToString();
//                        notABC.TellerId = (sysAPI.Result[0].UserId + 999).ToString();
//                        notABC.TellerName = sysAPI.Result[0].Tellername;
//                    }
//                    //var baseUrl = "http://xxsg.ebs-rcm.com/interface";
//                    var baseUrl = "https://revpay.ebs-rcm.com/BankPay/interface";
//                    var localUrl = $"Payment";
//                    //notABC.Hash = GenerateSHA512String(encryptedString);
//                    string jsonString = JsonConvert.SerializeObject(notABC);
//                    var clientUrl = string.Format($"{baseUrl}/{localUrl}").Trim();

//                    var client = new RestClient(clientUrl);

//                    var request = new RestRequest(Method.POST);
//                    request.AddHeader("Content-Type", "application/json");
//                    request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
//                    var response = client.Execute<PaymentNotificationABC>(request).Content;
//                    if (response.Contains("SUCCESSFULL") == true)
//                    {
//                        res = "{statuscode:200, statusmsg:Success}";
//                    }
//                    else
//                    {
//                        res = "{statuscode:200, statusmsg:Failure}";
//                    }
//                    var requestResponse = new SysRequestResponseLog
//                    {
//                        Request = request.ToString(),
//                        Response = response.ToString(),
//                        Datecreated = DateTime.Now
//                    };

//                    //Commit into the RequestResponse Log Table
//                    await _unitOfWork.RequestResponseLog.AddAsync(requestResponse);
//                    // var content = response.Content;
//                    //LagResponse model = JsonConvert.DeserializeObject<LagResponse>(content);
//                    //if (model.Status == "SUCCESS")

//                    //{
//                    //    await GeneralSqlClient.CompleteLagosInvoices(req.Referencenumber, model.WebGuid);
//                    //    outmsg = "Vehicle Pushed Successfully";
//                    //}
//                    //return res;
//                }
//                return Json(res);


//            }
//            catch (Exception ex)
//            {
//                return Json("{statuscode:500, statusmsg:" + ex.Message + ex.StackTrace + "}");
//                throw;
//            }
//        }
//        public string ABCNotification(string Ref)
//        {
//            try
//            {
//                string res = "failure";
//                PaymentNotificationABC notABC = new PaymentNotificationABC();
//                // var sysAPI = _unitOfWork.Invoices.GetInvoiceWithInvoiceUser    (Ref); // .VerifyInvoice(Ref);
//                var sysAPI = GeneralSqlClient.GetLagosNotifications(Ref);
//                if (sysAPI.Result.Count > 0)
//                {
//                    notABC.CbnCode = null;
//                    // notABC.ClientId = "517578446363033";
//                    notABC.ClientId = "288539206722584";
//                    notABC.Webguid = sysAPI.Result[0].AlternativeReferenceno;
//                    notABC.AmountPaid = sysAPI.Result[0].Amount.ToString();
//                    //  notABC.State = "XXSG";
//                    notABC.State = "LASG";
//                    notABC.CreditAccount = "0004650857";
//                    notABC.BankNote = "Vehicle_Licnese_" + sysAPI.Result[0].Referenceno;
//                    notABC.PosId = null;
//                    notABC.TransId = null;
//                    //notABC.Hash = GenerateSHA512String("PH5UVZG993ND6BMADS65" + notABC.Webguid + notABC.State + notABC.AmountPaid);
//                    notABC.Hash = GenerateSHA512String("GE620HIT5B3F0M7C5LOE" + notABC.Webguid + notABC.State + notABC.AmountPaid);
//                    notABC.PaymentChannel = "EgolePay";
//                    notABC.PaymentRef = sysAPI.Result[0].Referenceno.ToString() + "1";
//                    notABC.TerminalId = null;
//                    notABC.TransRef = null;
//                    notABC.SendBy = null;
//                    notABC.Date = sysAPI.Result[0].Date.ToString();
//                    notABC.TellerId = (sysAPI.Result[0].UserId + 999).ToString();
//                    notABC.TellerName = sysAPI.Result[0].Tellername;
//                }
//                //var baseUrl = "http://xxsg.ebs-rcm.com/interface";
//                var baseUrl = "https://revpay.ebs-rcm.com/BankPay/interface";
//                var localUrl = $"Payment";
//                //notABC.Hash = GenerateSHA512String(encryptedString);
//                string jsonString = JsonConvert.SerializeObject(notABC);
//                var clientUrl = string.Format($"{baseUrl}/{localUrl}").Trim();

//                var client = new RestClient(clientUrl);

//                var request = new RestRequest(Method.POST);
//                request.AddHeader("Content-Type", "application/json");
//                request.AddParameter("application/json", jsonString, ParameterType.RequestBody);
//                var response = client.Execute<PaymentNotificationABC>(request).Content;
//                if (response.Contains("SUCCESSFULL") == true)
//                {
//                    res = "{statuscode:200, statusmsg:Success}";
//                }
//                else
//                {
//                    res = "{statuscode:200, statusmsg:Failure}";
//                }
//                var requestResponse = new SysRequestResponseLog
//                {
//                    Request = request.ToString(),
//                    Response = response.ToString(),
//                    Datecreated = DateTime.Now
//                };

//                //Commit into the RequestResponse Log Table
//                _unitOfWork.RequestResponseLog.AddAsync(requestResponse);
//                // var content = response.Content;
//                //LagResponse model = JsonConvert.DeserializeObject<LagResponse>(content);
//                //if (model.Status == "SUCCESS")

//                //{
//                //    await GeneralSqlClient.CompleteLagosInvoices(req.Referencenumber, model.WebGuid);
//                //    outmsg = "Vehicle Pushed Successfully";
//                //}
//                return res;


//            }
//            catch (Exception ex)
//            {
//                return "{statuscode:500, statusmsg:" + ex.Message + ex.StackTrace + "}";
//                throw;
//            }
//        }
//        /// <summary>
//        ///     for state payment notifications
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        /// <remark>
//        /// sample response: {StatusCode="0",Message="Payment saved successfully",Reference= model.InvoiceReference}
//        /// </remark>
//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(PaymentNotificationResponseViewModel))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("notification/all")]
//        [HttpPost, Route("notification/state/{othercompany}")]
//        public async Task<IActionResult> NotificationGeneral(PaymentnotificationViewModel model, string othercompany)
//        {
//            try
//            {
//                General.LogToFile($"\nRequest:\n{JsonConvert.SerializeObject(model)}", generalNotification);
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                if (!string.IsNullOrEmpty(othercompany))
//                {
//                    var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

//                    var Vendors = (await _unitOfWork.PaymentVendor.FindAsync(p => p.ClientWhiteListedIP.Contains(remoteIpAddress))).ToList();

//                    var paymentVendor = Vendors.FirstOrDefault(c => c.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString() || i == "*"));

//                    if (paymentVendor == null)
//                    {
//                        //IsValidIP = true;
//                        return BadRequest(new { message = "Vendor Unknown" });
//                    }
//                }
//                if (othercompany == "ABC")
//                {
//                    //ABCNotification(model.InvoiceReference );
//                }

//                //check amount for equality
//                //if (Convert.ToDouble(model.total) != Convert.ToDouble(model)) // .Sum(s => Convert.ToDouble(s.amount ))))
//                //{
//                //    return BadRequest(new { message = "Total and Sum of services amount must be equal." });
//                //}

//                var sysAPI = await GeneralSqlClient.GetSYSAPIKeyRecord(model.passKey);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new { message = "Wrong passkey." });
//                }


//                var hash = General.GetHash(new string[] { model.InvoiceReference, model.passKey, string.Format("{0:0.00}", Convert.ToDouble(model.total)) });
//                var hashToBeRemoved = General.GetHash(new string[] { model.InvoiceReference, model.passKey, Convert.ToDouble(model.total).ToString() });
//                if (model.hash.ToUpper() == hash.ToUpper() || model.hash.ToUpper() == hashToBeRemoved.ToUpper())
//                {
//                    //return BadRequest("Incorrect hash code.");
//                }
//                else
//                {
//                    //return BadRequest(new { message = "Incorrect hash code." });
//                }

//                //save value to payment and complete invoice
//                var invoices = (await GeneralSqlClient.VerifyInvoicePaidOrUnpaid(model.InvoiceReference))?.Invoices;// _unitOfWork.Invoices.Find(i => i.ReferenceNo.ToUpper() == model.InvoiceReference.ToUpper()).ToList();
//                if (invoices == null)
//                {
//                    return BadRequest(new { message = "Wrong customer_id" });
//                }
//                else if (invoices.All(i => i.PaymentStatus))
//                {
//                    return Ok(new { message = "Payment has been updated before." });
//                }
//                else
//                {
//                    if (GeneralSqlClient.SaveNotification(model) /*await _apiRepository.SavePaymentNotification(model)*/)
//                    {
//                        var response = new PaymentNotificationResponseViewModel
//                        {
//                            StatusCode = "0",
//                            Message = "Payment saved successfully",
//                            Reference = model.InvoiceReference
//                        };

//                        //Pushing Data to RTVRS Nassarawa State
//                        if (othercompany == "NAS")
//                        {
//                            Functions fnc = new Functions((UnitOfWork)_unitOfWork);
//                            await fnc.PushToRTVRS(model.InvoiceReference);
//                        }
//                        General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                        return Ok(response);
//                    }
//                    else
//                    {
//                        General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                        return BadRequest(new { message = "payment could not be saved." });
//                    }
//                }

//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new { message = "An error has occured! Please try again" });
//            }

//        }


//        /// <summary>
//        /// for VFD payment notifications
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        /// <remark>
//        /// sample response: {StatusCode="0",Message="Payment saved successfully",Reference= model.InvoiceReference}
//        /// </remark>
//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(PaymentNotificationResponseViewModel))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("notification/VFD")]
//        //[HttpPost, Route("notification/{company}")] 
//        public async Task<IActionResult> NotificationGeneral()
//        {
//            string company = "VFD";
//            try
//            {
//                string requeststr = Request.ContentLength.ToString();

//                //PaymentnotificationViewModelVFD model;
//                PaymentnotificationViewModelVFD model = JsonConvert.DeserializeObject<PaymentnotificationViewModelVFD>(requeststr);
//                General.LogToFile($"\nRequest:\n{JsonConvert.SerializeObject(model)}", generalNotification);
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                if (!string.IsNullOrEmpty(company))
//                {
//                    var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();

//                    var Vendors = (await _unitOfWork.PaymentVendor.FindAsync(p => p.ClientWhiteListedIP.Contains(remoteIpAddress))).ToList();

//                    var paymentVendor = Vendors.FirstOrDefault(c => c.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString() || i == "*"));

//                    if (paymentVendor == null)
//                    {
//                        //IsValidIP = true;
//                        return BadRequest(new { message = "Vendor Unknown" });
//                    }
//                }

//                var foundReference = (await _unitOfWork.WalletHistory.FindAsync(w => w.ReferenceNo.ToUpper() == model.reference.ToUpper())).FirstOrDefault();
//                if (foundReference != null)
//                {
//                    //return BadRequest(new { message = "You are not allowed on this resource." });
//                    var response = new PaymentNotificationResponseViewModel
//                    {
//                        //StatusCode = "1",
//                        Message = "Payment with reference number " + model.reference + " has been recorded before.",
//                        Reference = model.reference
//                    };
//                    General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                    return Ok(response);
//                }

//                //////check amount for equality
//                ////if (Convert.ToDouble(model.total) != Convert.ToDouble(model.services.Sum(s => Convert.ToDouble(s.amount))))
//                ////{
//                ////    return BadRequest(new { message = "Total and Sum of services amount must be equal." });
//                ////}

//                //var sysAPI = await GeneralSqlClient.GetSYSAPIKeyRecord(model.passKey);
//                //if (sysAPI == null)
//                //{
//                //    return BadRequest(new { message = "Wrong passkey." });
//                //}


//                ////var hash = General.GetHash(new string[] { model.InvoiceReference, model.passKey, string.Format("{0:0.00}", Convert.ToDouble(model.total)) });
//                ////var hashToBeRemoved = General.GetHash(new string[] { model.InvoiceReference, model.passKey, Convert.ToDouble(model.total).ToString() });
//                //var hash = General.GetHash(new string[] { model.reference, model.passKey, string.Format("{0:0.00}", Convert.ToDouble(model.amount)) });
//                //var hashToBeRemoved = General.GetHash(new string[] { model.reference, model.passKey, Convert.ToDouble(model.amount).ToString() });
//                //if (model.hash.ToUpper() == hash.ToUpper() || model.hash.ToUpper() == hashToBeRemoved.ToUpper())
//                //{
//                //    //return BadRequest("Incorrect hash code.");
//                //}
//                //else
//                //{
//                //    return BadRequest(new { message = "Incorrect hash code." });
//                //}

//                ////save value to payment and complete invoice
//                //var invoices = (await GeneralSqlClient.VerifyInvoicePaidOrUnpaid(model.InvoiceReference))?.Invoices;// _unitOfWork.Invoices.Find(i => i.ReferenceNo.ToUpper() == model.InvoiceReference.ToUpper()).ToList();
//                //if (invoices == null)
//                //{
//                //    return BadRequest(new { message = "Wrong customer_id" });
//                //}
//                //else if (invoices.All(i => i.PaymentStatus))
//                //{
//                //    return Ok(new { message = "Payment has been updated before." });
//                //}
//                //else
//                //{
//                if (GeneralSqlClient.SaveNotification(model, company) /*await _apiRepository.SavePaymentNotification(model)*/)
//                {
//                    var response = new PaymentNotificationResponseViewModel
//                    {
//                        StatusCode = "0",
//                        Message = "Payment saved successfully",
//                        Reference = model.reference
//                    };
//                    General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                    return Ok(response);
//                }
//                else
//                {
//                    General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                    return BadRequest(new { message = "payment could not be saved." });
//                }
//                //}

//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new { message = "An error has occured! Please try again" });
//            }

//        }


//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(PaymentNotificationResponseViewModel))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("notification/Remita")]
//        public async Task<IActionResult> NotificationRemita(PaymentnotificationViewModelRemita model/* string company*/)
//        {
//            string company = "Remita";
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var normalizedName = company.ToUpper();
//                //var IsAllowed = Convert.ToBoolean(_appSetup?.FourCoreCredential?.AllowedCompany?.Split(',').Any(i => i.ToUpper() == normalizedName));

//                //if (!IsAllowed)
//                //{
//                //    return BadRequest(new { message = "You are not allowed on this resource." });
//                //}

//                var paymentVendor = (await _unitOfWork.PaymentVendor.FindAsync(p => p.PaymentVendorName.ToUpper() == normalizedName)).FirstOrDefault();
//                if (paymentVendor == null)
//                {
//                    return BadRequest(new { message = "You are not allowed on this resource." });
//                }

//                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
//                var ClientId = paymentVendor?.ClientId;
//                var ClientSecret = paymentVendor?.ClientSecret;
//                var PaymentVendorId = paymentVendor?.PaymentVendorId;

//                //check remoteIPAddress 
//                var IsValidIP = Convert.ToBoolean(_appSetup?.FourCoreCredential?.IsSetup) ?
//                    Convert.ToBoolean(paymentVendor?.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString())) : true;

//                if (IsValidIP)
//                {
//                    //var HASH = General.GetHash256(new string[] { model.InvoiceNumber, model.rrr, string.Format("{0:0.00}", Convert.ToDouble(model.amount)), model.orderRef }).ToUpper();

//                    //var Hash2 = General.GetHash256ByASCII(new string[] { model.InvoiceNumber, model.rrr, string.Format("{0:0.00}", Convert.ToDouble(model.amount)), model.orderRef }, ClientId, ClientSecret).ToUpper();

//                    //if (model.MAC.ToUpper() != Hash2)
//                    //{
//                    //    return BadRequest(new { message = "Invalid MAC" });
//                    //}

//                    var Invoices = (await GeneralSqlClient.VerifyInvoicePaidOrUnpaidSpecial(model.rrr))?.Invoices;
//                    if (Invoices == null || Invoices.Count < 1)
//                    {
//                        return BadRequest(new { message = "Wrong customer_id" });
//                    }
//                    else if (Invoices.FirstOrDefault().PaymentStatus)
//                    {
//                        return Ok(new { message = "Payment has been updated before." });
//                    }

//                    //check amount for equality
//                    double number = 0.00;
//                    if (Double.TryParse(model.amount.ToString(), out number))
//                    {
//                        if (Convert.ToDouble(number) != Convert.ToDouble(Invoices.Sum(s => Convert.ToDouble(s.Amount))))
//                        {
//                            return Ok(new { message = $"Amount less than payable on the invoice reference {model.rrr}." });
//                        }
//                    }
//                    else
//                    {
//                        return BadRequest(new { message = $"Amount '{model.amount}' is not money." });
//                    }

//                    //save value to payment and complete invoice
//                    //pay
//                    //var RefNo = Invoices.FirstOrDefault()?.ReferenceNo;
//                    var RefNo = model.rrr;
//                    //CultureInfo provider = CultureInfo.InvariantCulture;
//                    //DateTime dateTime12 = DateTime.ParseExact(model.transactiondate, "dd/mm/yyyy", provider);
//                    //var dbModel = new PaymentnotificationViewModel { date = model.transactiondate, InvoiceReference = RefNo, total = model.amount.ToString(), source = "BANK/POS" };
//                    string formatedDate = model.transactiondate.Substring(3, 2) + "/" + model.transactiondate.Substring(0, 2) + "/" + model.transactiondate.Substring(6, 4);
//                    var dbModel = new PaymentnotificationViewModel { date = formatedDate, InvoiceReference = RefNo, total = model.amount.ToString(), source = "BANK/POS" };
//                    if (GeneralSqlClient.SaveNotification(dbModel, RefNo, RefNo, false, PaymentVendorId) /*await _apiRepository.SavePaymentNotification(model)*/)
//                    {
//                        var response = new PaymentNotificationResponseViewModel
//                        {
//                            StatusCode = "0",
//                            Message = "Payment saved successfully",
//                            Reference = model.rrr
//                        };
//                        General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                        return Ok(response);
//                    }
//                    else
//                    {
//                        General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                        return BadRequest(new { message = "payment could not be saved." });
//                    }

//                }
//                else
//                {
//                    return BadRequest(new { message = "You are not allowed to use this resource." });
//                }
//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new { message = "An error has occured! Please try again" });
//            }

//        }

//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(PaymentNotificationResponseViewModel))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("notification/{company}")]
//        public async Task<IActionResult> NotificationGeneral(PaymentnotificationViewModelOYO model, string company)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
//                var IsValidIP = true;// _appSetup.WhiteListedIPs.Any(c=>c.IP.Split(',').Any(i=>i==remoteIpAddress.ToString()));
//                string ReferenceNo = company.ToUpper() == "OSUN" ? model.PaymentRefNo : model.UniqueReferenceNo;

//                var Vendors = (await _unitOfWork.PaymentVendor.FindAsync(p => p.ClientWhiteListedIP.Contains(remoteIpAddress))).ToList();

//                var paymentVendor = Vendors.FirstOrDefault(c => c.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString()));

//                if (paymentVendor != null)
//                {
//                    IsValidIP = true;
//                }

//                if (IsValidIP)
//                {
//                    var UseAlternateRefNo = company.ToUpper() == "DREAMLABS";
//                    var Invoices = (await GeneralSqlClient.VerifyInvoicePaidOrUnpaid(ReferenceNo, UseAlternateRefNo.ToString()))?.Invoices;

//                    if (Invoices == null || Invoices.Count == 0)
//                    {
//                        return BadRequest(new { message = "Wrong customer_id" });
//                    }
//                    else if (Invoices.FirstOrDefault().PaymentStatus)
//                    {
//                        return Ok(new { message = "Payment has been updated before." });
//                    }

//                    //check amount for equality
//                    double number = 0.0;
//                    if (Double.TryParse(model.Amount, out number))
//                    {
//                        if (Convert.ToDouble(number) < Convert.ToDouble(Invoices.Sum(s => Convert.ToDouble(s.Amount))))
//                        {
//                            return Ok(new { message = $"Amount not equal to payable on the invoice reference {ReferenceNo}." });
//                        }
//                    }
//                    else
//                    {
//                        return BadRequest(new { message = $"Amount '{model.Amount}' is not money." });
//                    }

//                    //save value to payment and complete invoice
//                    //pay
//                    //var dbModel = new PaymentnotificationViewModel { date = model.TransactionDate, InvoiceReference = ReferenceNo, total = model.Amount, source = "BANK/POS" };
//                    var dbModel = new PaymentnotificationViewModel { date = DateTime.Now.ToString(), InvoiceReference = ReferenceNo, total = model.Amount, source = "BANK/POS" };

//                    if (GeneralSqlClient.SaveNotification(dbModel, model.RevenueCode, ReferenceNo, false, paymentVendor?.PaymentVendorId) /*await _apiRepository.SavePaymentNotification(model)*/)
//                    {
//                        var response = new PaymentNotificationResponseViewModel
//                        {
//                            StatusCode = "0",
//                            Message = "Payment saved successfully",
//                            Reference = ReferenceNo
//                        };
//                        General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                        return Ok(response);
//                    }
//                    else
//                    {
//                        General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                        return BadRequest(new { message = "payment could not be saved." });
//                    }

//                }
//                else
//                {
//                    return BadRequest(new { message = "You are not allowed to use this resource." });
//                }
//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new { message = "An error has occured! Please try again" });
//            }

//        }

//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(PaymentNotificationResponseViewModel))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("Callback/{company}")]
//        public async Task<IActionResult> NotificationGeneral(FourCorePaymentNotificationViewModel model, string company)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var normalizedName = company.ToUpper();
//                //var IsAllowed = Convert.ToBoolean(_appSetup?.FourCoreCredential?.AllowedCompany?.Split(',').Any(i => i.ToUpper() == normalizedName));

//                //if (!IsAllowed)
//                //{
//                //    return BadRequest(new { message = "You are not allowed on this resource." });
//                //}

//                var paymentVendor = (await _unitOfWork.PaymentVendor.FindAsync(p => p.PaymentVendorName.ToUpper() == normalizedName)).FirstOrDefault();
//                if (paymentVendor == null)
//                {
//                    return BadRequest(new { message = "You are not allowed on this resource." });
//                }

//                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
//                var ClientId = paymentVendor?.ClientId;
//                var ClientSecret = paymentVendor?.ClientSecret;

//                //check remoteIPAddress 
//                var IsValidIP = Convert.ToBoolean(_appSetup?.FourCoreCredential?.IsSetup) ?
//                    Convert.ToBoolean(paymentVendor?.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString())) : true;

//                if (IsValidIP)
//                {
//                    var HASH = General.GetHash256(new string[] { model.InvoiceNumber, model.PaymentRef, string.Format("{0:0.00}", Convert.ToDouble(model.AmountPaid)), model.RequestReference }).ToUpper();

//                    var Hash2 = General.GetHash256ByASCII(new string[] { model.InvoiceNumber, model.PaymentRef, string.Format("{0:0.00}", Convert.ToDouble(model.AmountPaid)), model.RequestReference }, ClientId, ClientSecret).ToUpper();

//                    if (model.MAC.ToUpper() != Hash2)
//                    {
//                        return BadRequest(new { message = "Invalid MAC" });
//                    }

//                    var Invoices = (await GeneralSqlClient.VerifyInvoicePaidOrUnpaidSpecial(model.InvoiceNumber))?.Invoices;
//                    if (Invoices == null || Invoices.Count < 1)
//                    {
//                        return BadRequest(new { message = "Wrong customer_id" });
//                    }
//                    else if (Invoices.FirstOrDefault().PaymentStatus)
//                    {
//                        return Ok(new { message = "Payment has been updated before." });
//                    }

//                    //check amount for equality
//                    double number = 0.00;
//                    if (Double.TryParse(model.AmountPaid, out number))
//                    {
//                        if (Convert.ToDouble(number) != Convert.ToDouble(Invoices.Sum(s => Convert.ToDouble(s.Amount))))
//                        {
//                            return Ok(new { message = $"Amount less than payable on the invoice reference {model.InvoiceNumber}." });
//                        }
//                    }
//                    else
//                    {
//                        return BadRequest(new { message = $"Amount '{model.AmountPaid}' is not money." });
//                    }

//                    //save value to payment and complete invoice
//                    //pay
//                    var RefNo = Invoices.FirstOrDefault()?.ReferenceNo;
//                    var dbModel = new PaymentnotificationViewModel { date = model.TransactionDate, InvoiceReference = RefNo, total = model.AmountPaid, source = "BANK/POS" };
//                    if (GeneralSqlClient.SaveNotification(dbModel, RefNo, RefNo, true) /*await _apiRepository.SavePaymentNotification(model)*/)
//                    {
//                        var response = new PaymentNotificationResponseViewModel
//                        {
//                            StatusCode = "0",
//                            Message = "Payment saved successfully",
//                            Reference = model.InvoiceNumber
//                        };
//                        General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                        return Ok(response);
//                    }
//                    else
//                    {
//                        General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                        return BadRequest(new { message = "payment could not be saved." });
//                    }

//                }
//                else
//                {
//                    return BadRequest(new { message = "You are not allowed to use this resource." });
//                }
//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new { message = "An error has occured! Please try again" });
//            }

//        }

//        //[Authorize()]
//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpGet, Route("verify/{Ref}")]
//        //[HttpGet, Route("verify/{Ref}/{Vendor}")]
//        public async Task<IActionResult> Verify(string Ref, string vendor = "null", string status = "ok")
//        {

//            try
//            {
//                //General.LogToFile($"\nRequest:\n{Ref}", generalNotification);

//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }
//                //check amount for type equality

//                //check for correct hash=InvoiceReference+passKey+total
//                //+companyId
//                if (vendor == "null" && Ref.Substring(0, 3) == "704")
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "payment cannot be made here!", Status = "1" });
//                }
//                if (vendor == "null" && Ref.Substring(0, 3) == "132")
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "payment cannot be made here!", Status = "1" });
//                }
//                var sysAPI = await _apiRepository.VerifyInvoice(Ref);

//                //var sysAPI =GeneralSqlClient.VerifyInvoice(Ref);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "Invoice does not exist or has been paid", Status = "1" });
//                }
//                else
//                {

//                    return Ok(new { CustomerInformationResponse = sysAPI });
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new CustomerInformationResponse { Message = "Critical Error, Contact Administrator", Status = "1" });
//            }

//        }


//        [ApiExplorerSettings(IgnoreApi = false)]
//        //[HttpGet, Route("verify/{Ref}")]
//        [HttpGet, Route("verify/{Ref}/{Vendor}/{Status}")]
//        public async Task<IActionResult> VerifyNassarawa(string Ref, string vendor, string status = "ok")
//        {

//            try
//            {
//                //General.LogToFile($"\nRequest:\n{Ref}", generalNotification);

//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }
//                //check amount for type equality

//                //check for correct hash=InvoiceReference+passKey+total
//                //+companyId
//                if (vendor == "null" && Ref.Substring(0, 3) == "704")
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "payment cannot be made here!", Status = "1" });
//                }
//                var sysAPI = await _apiRepository.VerifyInvoice(Ref);

//                //var sysAPI =GeneralSqlClient.VerifyInvoice(Ref);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "Invoice does not exist or has been paid", Status = "1" });
//                }
//                else
//                {

//                    return Ok(new { CustomerInformationResponse = sysAPI });
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new CustomerInformationResponse { Message = "Critical Error, Contact Administrator", Status = "1" });
//            }

//        }


//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpGet, Route("verify/{Ref}/{StateCode}")]
//        public async Task<IActionResult> Verify(string Ref, string StateCode)
//        {

//            try
//            {
//                //General.LogToFile($"\nRequest:\n{Ref}", generalNotification);

//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }
//                //check amount for type equality

//                //check for correct hash=InvoiceReference+passKey+total
//                //+companyId
//                int InvState = GeneralSqlClient.GetStateIDByCode(StateCode);
//                // var sysAPI = null;

//                var sysAPI = await _apiRepository.VerifyInvoice(Ref);


//                if (InvState.ToString() != sysAPI.ThirdPartyCode || InvState == 0)
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "You have selected the wrong state payment", Status = "1" });
//                }

//                //var sysAPI =GeneralSqlClient.VerifyInvoice(Ref);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "Invoice does not exist or has been paid", Status = "1" });
//                }
//                else
//                {
//                    return Ok(new { CustomerInformationResponse = sysAPI });
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new CustomerInformationResponse { Message = "Critical Error, Contact Administrator", Status = "1" });
//            }

//        }

//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpGet, Route("verifyOGSG/{Ref}/{StateCode}")]
//        public async Task<IActionResult> VerifyOGSG(string Ref, string StateCode)
//        {

//            try
//            {
//                //General.LogToFile($"\nRequest:\n{Ref}", generalNotification);

//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }
//                //check amount for type equality

//                //check for correct hash=InvoiceReference+passKey+total
//                //+companyId
//                // int InvState = GeneralSqlClient.GetStateIDByCode(StateCode);
//                // var sysAPI = null;

//                var sysAPI = await _apiRepository.VerifyOGSGInvoice(Ref);


//                //if (InvState.ToString() != sysAPI.ThirdPartyCode || InvState == 0)
//                //{
//                //    return BadRequest(new CustomerInformationResponse { Message = "You have selected the wrong state payment", Status = "1" });
//                //}

//                // var sysAPI = await GeneralSqlClient.VerifyOGSGInvoice(Ref);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "Invoice does not exist.", Status = "1" });
//                }
//                else if (sysAPI.Status.ToString() == "1")
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "Invoice has been paid.", Status = "-1" });
//                }

//                else
//                {
//                    return Ok(new { CustomerInformationResponse = sysAPI });
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new CustomerInformationResponse { Message = "Critical Error, Contact Administrator", Status = "1" });
//            }

//        }

//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpGet, Route("verifyOGSGOthers/{Ref}")]
//        public async Task<IActionResult> VerifyOGSGOthers(string Ref)
//        {

//            try
//            {
//                //General.LogToFile($"\nRequest:\n{Ref}", generalNotification);

//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }
//                //check amount for type equality

//                //check for correct hash=InvoiceReference+passKey+total
//                //+companyId
//                // int InvState = GeneralSqlClient.GetStateIDByCode(StateCode);
//                // var sysAPI = null;

//                // var sysAPI = await _apiRepository.VerifyOGSGInvoice(Ref);


//                //if (InvState.ToString() != sysAPI.ThirdPartyCode || InvState == 0)
//                //{
//                //    return BadRequest(new CustomerInformationResponse { Message = "You have selected the wrong state payment", Status = "1" });
//                //}

//                var sysAPI = await GeneralSqlClient.VerifyOGSGInvoiceOthers(Ref);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new OGSGPaymentOthers { Message = "Invoice does not exist or has been paid" });
//                }
//                else
//                {
//                    return Ok(new { OGSGPaymentOthers = sysAPI });
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new OGSGPaymentOthers { Message = "Critical Error, Contact Administrator" });
//            }

//        }

//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpPost, Route("verify/ussd")]
//        public async Task<IActionResult> VerifyUssd(UssdVerify model)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var sysAPI = await _apiRepository.VerifyInvoice(model.customerRef);

//                if (sysAPI == null)
//                {
//                    var response = new UssdVerifyResponse
//                    {
//                        amount = sysAPI?.TotalAmount,
//                        customerName = $"{sysAPI?.FirstName} {sysAPI?.LastName}",
//                        displayMessage = $"Invoice does not exist or has been paid",
//                        //passBackReference = sysAPI?.CustReference,
//                        responseCode = "01"
//                    };
//                    return BadRequest(response);
//                }
//                else
//                {
//                    var name = $"{sysAPI?.FirstName} {sysAPI?.LastName}";

//                    var response = new UssdVerifyResponse
//                    {
//                        amount = sysAPI.TotalAmount,
//                        customerName = $"{sysAPI?.FirstName} {sysAPI?.LastName}",
//                        displayMessage = $"Payment for {name}",
//                        //passBackReference=sysAPI?.CustReference,
//                        responseCode = "00"
//                    };

//                    return Ok(response);
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new UssdVerifyResponse { displayMessage = "Critical Error, Contact Administrator", responseCode = "01" });
//            }

//        }


//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(UssdNotificationResponse))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("notification/ussd/{company}")]
//        public async Task<IActionResult> NotificationUssd(UssdNotification model, string company)
//        {
//            //string generalNotification = "notification/all";
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var paymentVendor = (await _unitOfWork.PaymentVendor.FindAsync(p => p.ClientId == model.merchantId)).FirstOrDefault();
//                if (paymentVendor == null)
//                {
//                    return BadRequest(new UssdNotificationResponse { responseMessage = $"Invalid MerchantId {model.merchantId}", responseCode = "01" });
//                }

//                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
//                var IsValidIP = Convert.ToBoolean(_appSetup?.RoutePermission?.IsUssdNotificationEnabled) ?
//                    Convert.ToBoolean(paymentVendor?.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString())) : true;

//                string ReferenceNo = model?.customerRef; ;// model?.traceId;

//                //check remoteIPAddress 
//                if (IsValidIP)
//                {
//                    var stringL = General.ArrayToString(new string[] { model.paymentReference, model.customerRef, model.responseCode, model.merchantId,
//                            string.Format("{0:0.00}", Convert.ToDouble(model.amount)),"|" ,paymentVendor?.ClientAPIKey}
//                        );

//                    var hash = General.GetSha256Managed(stringL);

//                    if (hash.ToUpper() != model.hash.ToUpper())
//                    {
//                        return BadRequest(new UssdNotificationResponse { responseMessage = $"Wrong hash {model.hash}", responseCode = "01" });
//                    }

//                    var Invoices = (await GeneralSqlClient.VerifyInvoicePaidOrUnpaid(ReferenceNo))?.Invoices;
//                    if (Invoices == null || Invoices.Count == 0)
//                    {
//                        return BadRequest(new UssdNotificationResponse { responseMessage = "Wrong customer_id", responseCode = "01" });
//                    }
//                    else if (Invoices.FirstOrDefault().PaymentStatus)
//                    {
//                        return Ok(new { message = "Payment has been updated before.", responseCode = "01" });
//                    }

//                    //check amount for equality
//                    double number = 0.0;
//                    if (Double.TryParse(model.amount, out number))
//                    {
//                        if (Convert.ToDouble(number) < Convert.ToDouble(Invoices.Sum(s => Convert.ToDouble(s.Amount))))
//                        {
//                            return Ok(new UssdNotificationResponse { responseMessage = $"Amount not equal to payable on the invoice reference {ReferenceNo}.", responseCode = "01" });
//                        }
//                    }
//                    else
//                    {
//                        return BadRequest(new UssdNotificationResponse { responseMessage = $"Amount '{model.amount}' is not money.", responseCode = "01" });
//                    }

//                    //save value to payment and complete invoice
//                    //pay
//                    var dbModel = new PaymentnotificationViewModel { date = model.transactionDate, InvoiceReference = ReferenceNo, total = model.amount, source = $"{model.channel}-{model.currency}" };
//                    if (GeneralSqlClient.SaveNotification(dbModel, "", model.paymentReference, false, paymentVendor?.PaymentVendorId) /*await _apiRepository.SavePaymentNotification(model)*/)
//                    {
//                        var response = new UssdNotificationResponse
//                        {
//                            responseCode = "00",
//                            responseMessage = "Payment saved successfully"
//                        };
//                        //General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                        return Ok(response);
//                    }
//                    else
//                    {
//                        //General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                        return BadRequest(new UssdNotificationResponse { responseMessage = "payment could not be saved.", responseCode = "01" });
//                    }

//                }
//                else
//                {
//                    return BadRequest(new UssdNotificationResponse { responseMessage = "You are not allowed to use this resource.", responseCode = "01" });
//                }
//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new UssdNotificationResponse { responseMessage = "An error has occured! Please try again", responseCode = "01" });
//            }

//        }



//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpPost, Route("verify/ussd/renewal")]
//        public async Task<IActionResult> VerifyUssdRenewal(UssdVerify model)
//        {

//            try
//            {
//                var paymentVendor = (await _unitOfWork.PaymentVendor.FindAsync(p => p.ClientId == model.merchantId)).FirstOrDefault();
//                if (paymentVendor == null)
//                {
//                    return BadRequest(new UssdNotificationResponse { responseMessage = $"Invalid MerchantId {model.merchantId}", responseCode = "01" });
//                }

//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                //confirm the ref sent back is valid by structure
//                if (!model.customerRef.Contains("+"))
//                {
//                    return BadRequest(new UssdVerifyResponse { displayMessage = "Invalid customer reference" });
//                }

//                var receivedIds = model.customerRef.Split('+').ToList();
//                var ProductCode = receivedIds.FirstOrDefault(r => r.ToString().Length == 2);
//                var AssetCode = Convert.ToInt32(receivedIds.FirstOrDefault(r => r != ProductCode));

//                //check if reference contains expected code
//                if (Convert.ToInt32(ProductCode) < 1)
//                {
//                    return BadRequest(new UssdVerifyResponse { displayMessage = "Invalid product code in customer reference" });
//                }

//                var asset = await _unitOfWork.Assets.GetAssetWithActiveVehicleByAssetID(AssetCode);

//                if (Convert.ToInt32(asset?.Vehicle?.VehicleCategory?.VehicleCategoryId) < 1)
//                {
//                    return BadRequest(new UssdVerifyResponse { displayMessage = "Invalid vehicle details on customer reference" });
//                }
//                var invoiceStateServiceRate = await _unitOfWork.StateServiceRates.GetByAlternateCode(ProductCode, asset.Vehicle.VehicleCategory.VehicleCategoryId);

//                //check if reference contains expected code
//                if (asset == null)
//                {
//                    return BadRequest(new UssdVerifyResponse { displayMessage = "Invalid customer code in customer reference" });
//                }
//                else if (asset?.Vehicle?.VehicleOwnerships?.TaxPayer == null)
//                {
//                    return BadRequest(new UssdVerifyResponse { displayMessage = "Invalid customer details" });
//                }

//                if (invoiceStateServiceRate == null)
//                {
//                    return BadRequest(new UssdVerifyResponse { displayMessage = "Unknown product code in customer reference" });
//                }

//                //create invoice with given asset
//                var RefNo = this.GenerateReferenceNo(10);
//                var companyUser = await _unitOfWork.Users.GetSysUsers("1");
//                var invoiceState = await _unitOfWork.States.GetAsync(30);
//                var invoiceTransactionType = await _unitOfWork.TransactionTypes.GetAsync(1);

//                var Invoice = new IgrInvoices
//                {
//                    AlternativeReferenceNo = model.customerRef,
//                    Amount = Convert.ToDouble(invoiceStateServiceRate?.ServiceRate),
//                    Asset = asset,

//                    DateCreated = DateTime.Now,
//                    InvoiceState = invoiceState,
//                    ReferenceNo = RefNo,
//                    StateServiceRate = invoiceStateServiceRate,
//                    TaxPayer = asset?.TaxPayer,
//                    TransactionType = invoiceTransactionType,
//                    User = companyUser
//                };

//                var dbInvoice = await _unitOfWork.Invoices.AddSaveAsyncAPI(Invoice);

//                var sysAPI = await _apiRepository.VerifyInvoice(dbInvoice.ReferenceNo);

//                if (sysAPI == null)
//                {
//                    var response = new UssdVerifyResponse
//                    {
//                        amount = sysAPI?.TotalAmount,
//                        customerName = $"{sysAPI?.FirstName} {sysAPI?.LastName}",
//                        displayMessage = $"Invoice does not exist or has been paid",
//                        passBackReference = sysAPI?.CustReference,
//                        responseCode = "01"
//                    };
//                    return BadRequest(response);
//                }
//                else
//                {
//                    var name = $"{sysAPI?.FirstName} {sysAPI?.LastName}";

//                    var response = new UssdVerifyResponse
//                    {
//                        amount = sysAPI.TotalAmount,
//                        customerName = $"{sysAPI?.FirstName} {sysAPI?.LastName}",
//                        displayMessage = $"Payment for {name}",
//                        passBackReference = sysAPI?.CustReference,
//                        responseCode = "00"
//                    };

//                    return Ok(response);
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new UssdVerifyResponse { displayMessage = "Critical Error, Contact Administrator", responseCode = "01" });
//            }

//        }


//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(UssdNotificationResponse))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("notification/ussd/renewal/{company}")]
//        public async Task<IActionResult> NotificationUssdRenewal(UssdNotification model, string company)
//        {
//            //string generalNotification = "notification/all";
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var paymentVendor = (await _unitOfWork.PaymentVendor.FindAsync(p => p.ClientId == model.merchantId)).FirstOrDefault();
//                if (paymentVendor == null)
//                {
//                    return BadRequest(new UssdNotificationResponse { responseMessage = $"Invalid MerchantId {model.merchantId}", responseCode = "01" });
//                }

//                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
//                var IsValidIP = Convert.ToBoolean(_appSetup?.RoutePermission?.IsUssdNotificationEnabled) ?
//                    Convert.ToBoolean(paymentVendor?.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString())) : true;

//                string ReferenceNo = model?.traceId;

//                //check remoteIPAddress 
//                if (IsValidIP)
//                {
//                    var stringL = General.ArrayToString(new string[] { model.paymentReference, model.customerRef, model.responseCode, model.merchantId,
//                            string.Format("{0:0.00}", Convert.ToDouble(model.amount)),"|" ,paymentVendor?.ClientAPIKey}
//                        );

//                    var hash = General.GetSha256Managed(stringL);

//                    if (hash.ToUpper() != model.hash.ToUpper())
//                    {
//                        return BadRequest(new UssdNotificationResponse { responseMessage = $"Wrong hash {model.hash}", responseCode = "01" });
//                    }

//                    var Invoices = (await GeneralSqlClient.VerifyInvoicePaidOrUnpaid(ReferenceNo))?.Invoices;
//                    if (Invoices == null || Invoices.Count == 0)
//                    {
//                        return BadRequest(new UssdNotificationResponse { responseMessage = "Wrong customer_id", responseCode = "01" });
//                    }
//                    else if (Invoices.FirstOrDefault().PaymentStatus)
//                    {
//                        return Ok(new { message = "Payment has been updated before.", responseCode = "01" });
//                    }

//                    //check amount for equality
//                    double number = 0.0;
//                    if (Double.TryParse(model.amount, out number))
//                    {
//                        if (Convert.ToDouble(number) < Convert.ToDouble(Invoices.Sum(s => Convert.ToDouble(s.Amount))))
//                        {
//                            return Ok(new UssdNotificationResponse { responseMessage = $"Amount not equal to payable on the invoice reference {ReferenceNo}.", responseCode = "01" });
//                        }
//                    }
//                    else
//                    {
//                        return BadRequest(new UssdNotificationResponse { responseMessage = $"Amount '{model.amount}' is not money.", responseCode = "01" });
//                    }

//                    //save value to payment and complete invoice
//                    //pay
//                    var dbModel = new PaymentnotificationViewModel { date = model.transactionDate, InvoiceReference = ReferenceNo, total = model.amount, source = $"{model.channel}-{model.currency}" };
//                    if (GeneralSqlClient.SaveNotification(dbModel, "", model.paymentReference, false, paymentVendor?.PaymentVendorId) /*await _apiRepository.SavePaymentNotification(model)*/)
//                    {
//                        var response = new UssdNotificationResponse
//                        {
//                            responseCode = "00",
//                            responseMessage = "Payment saved successfully"
//                        };
//                        //General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                        return Ok(response);
//                    }
//                    else
//                    {
//                        //General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                        return BadRequest(new UssdNotificationResponse { responseMessage = "payment could not be saved.", responseCode = "01" });
//                    }

//                }
//                else
//                {
//                    return BadRequest(new UssdNotificationResponse { responseMessage = "You are not allowed to use this resource.", responseCode = "01" });
//                }
//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new UssdNotificationResponse { responseMessage = "An error has occured! Please try again", responseCode = "01" });
//            }

//        }

//        [ApiExplorerSettings(IgnoreApi = false)]
//        [HttpGet, Route("verifyRefwithItem/{Ref}/{item}")]
//        public async Task<IActionResult> VerifyInvoiceByItem(string Ref, string item)
//        {

//            try
//            {
//                //General.LogToFile($"\nRequest:\n{Ref}", generalNotification);

//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }
//                //check amount for type equality

//                //check for correct hash=InvoiceReference+passKey+total
//                //+companyId
//                var sysAPI = await _apiRepository.VerifyInvoiceByItemRefNum(Ref, item);

//                //var sysAPI =GeneralSqlClient.VerifyInvoice(Ref);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new CustomerInformationResponse { Message = "Invoice does not exist or has been paid", Status = "1" });
//                }
//                else
//                {
//                    return Ok(new { CustomerInformationResponse = sysAPI });
//                }


//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new CustomerInformationResponse { Message = "Critical Error, Contact Administrator", Status = "1" });
//            }

//        }
//        [ApiExplorerSettings(IgnoreApi = false)]
//        [ProducesResponseType(200, Type = typeof(PaymentNotificationResponseViewModel))]
//        [ProducesResponseType(400, Type = typeof(string))]
//        [HttpPost, Route("notification/mfb")]
//        [HttpPost, Route("notification/OGSG")]
//        public async Task<IActionResult> NotificationMFB(PaymentnotificationViewModel model, string username)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                {
//                    return BadRequest(ModelState);
//                }

//                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
//                var IsValidIP = _appSetup.WhiteListedIPs.Any(c => c.IP.Split(',').Any(i => i == remoteIpAddress.ToString()));
//                string ReferenceNo = model.InvoiceReference;
//                var APIKey = model.passKey;
//                var Vendors = (await _unitOfWork.PaymentVendor.FindAsync(p => p.ClientWhiteListedIP.Contains(remoteIpAddress))).ToList();

//                var paymentVendor = Vendors.FirstOrDefault(c => c.ClientWhiteListedIP.Split(',').Any(i => i == remoteIpAddress.ToString()));

//                if (paymentVendor != null)
//                {
//                    IsValidIP = true;
//                }
//                var sysAPI = await GeneralSqlClient.GetSYSAPIKeyRecord(model.passKey);
//                if (sysAPI == null)
//                {
//                    return BadRequest(new { message = "Wrong passkey." });
//                }
//                var hash = General.GetHash(new string[] { model.InvoiceReference, model.passKey, string.Format("{0:0.00}", Convert.ToDouble(model.total)) });
//                var hashToBeRemoved = General.GetHash(new string[] { model.InvoiceReference, model.passKey, Convert.ToDouble(model.total).ToString() });
//                if (model.hash.ToUpper() == hash.ToUpper() || model.hash.ToUpper() == hashToBeRemoved.ToUpper())
//                {
//                    //return BadRequest("Incorrect hash code.");
//                }
//                else
//                {
//                    return BadRequest(new { message = "Incorrect hash code." });
//                }

//                if (IsValidIP)
//                {

//                    var Invoices = (await GeneralSqlClient.VerifyOGSGInvoiceOthers(ReferenceNo));

//                    if (Invoices == null || Invoices.Count == 0)
//                    {
//                        return BadRequest(new { message = "Wrong customer_id or payment is already made." });
//                    }


//                    //check amount for equality
//                    double number = 0.0;
//                    if (username == "OGSG")
//                    {
//                        if (Double.TryParse(model.total, out number))
//                        {
//                            if (Convert.ToDouble(number) < Convert.ToDouble(Invoices[0].OGSGAmount))
//                            {
//                                return Ok(new { message = $"Amount not equal to payable on the invoice." });
//                            }
//                        }
//                        else
//                        {
//                            return BadRequest(new { message = $"Amount '{model.total}' is not money." });
//                        }
//                    }
//                    else
//                    {
//                        if (Double.TryParse(model.total, out number))
//                        {
//                            if (Convert.ToDouble(number) < Convert.ToDouble(Invoices[0].TotalFee))
//                            {
//                                return Ok(new { message = $"Amount not equal to payable on the invoice." });
//                            }
//                        }
//                        else
//                        {
//                            return BadRequest(new { message = $"Amount '{model.total}' is not money." });
//                        }
//                    }

//                    //save value to payment and complete invoice
//                    //pay
//                    //var dbModel = new PaymentnotificationViewModel { date = model.TransactionDate, InvoiceReference = ReferenceNo, total = model.Amount, source = "BANK/POS" };
//                    var dbModel = new PaymentnotificationViewModel { date = DateTime.Now.ToString(), InvoiceReference = ReferenceNo, total = model.total, source = "BANK" };

//                    if (GeneralSqlClient.SaveNotificationMFB(dbModel, username) /*await _apiRepository.SavePaymentNotification(model)*/)
//                    {
//                        var response = new PaymentNotificationResponseViewModel
//                        {
//                            StatusCode = "0",
//                            Message = "Payment saved successfully",
//                            Reference = ReferenceNo
//                        };
//                        General.LogToFile($"\nResponse:\n{JsonConvert.SerializeObject(response)}", generalNotification);

//                        return Ok(response);
//                    }
//                    else
//                    {
//                        General.LogToFile($"\nResponse:\npayment could not be saved.", generalNotification);
//                        return BadRequest(new { message = "payment could not be saved." });
//                    }

//                }
//                else
//                {
//                    return BadRequest(new { message = "You are not allowed to use this resource." });
//                }
//            }
//            catch (Exception exc)
//            {
//                return StatusCode(500, new { message = "An error has occured! Please try again" });
//            }

//        }
//    }

//}