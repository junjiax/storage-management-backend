using System.Security.Cryptography;
using System.Text;
using dotnet_backend.DTOs.Payment;
using dotnet_backend.Interfaces;
using dotnet_backend.Libraries;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace dotnet_backend.Services
{
	public class VnPayService : IVnPayService
	{
		private readonly IConfiguration _configuration;

		public VnPayService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string CreatePaymentUrl(PaymentInformationDto model, string ipAddress)
		{
			var vnpConfig = _configuration.GetSection("Vnpay");
			var callbackUrl = _configuration.GetSection("PaymentCallBack")["ReturnUrl"];
			var vnp_TmnCode = vnpConfig["TmnCode"] ?? string.Empty;
			var vnp_HashSecret = vnpConfig["HashSecret"] ?? string.Empty;
			var vnp_Url = vnpConfig["BaseUrl"] ?? string.Empty;
			var vnp_Version = vnpConfig["Version"] ?? "2.1.0";
			var vnp_Command = vnpConfig["Command"] ?? "pay";
			var vnp_CurrCode = vnpConfig["CurrCode"] ?? "VND";
			var vnp_Locale = vnpConfig["Locale"] ?? "vn";

			var orderId = model.OrderId; 
			var createDate = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

			var lib = new VnPayLibrary();
			lib.AddRequestData("vnp_Version", vnp_Version);
			lib.AddRequestData("vnp_Command", vnp_Command);
			lib.AddRequestData("vnp_TmnCode", vnp_TmnCode);
			lib.AddRequestData("vnp_Amount", ((long)(model.Amount * 100)).ToString());
			lib.AddRequestData("vnp_BankCode", "");
			lib.AddRequestData("vnp_CreateDate", createDate);
			lib.AddRequestData("vnp_CurrCode", vnp_CurrCode);
			lib.AddRequestData("vnp_IpAddr", ipAddress);
			lib.AddRequestData("vnp_Locale", vnp_Locale);
			lib.AddRequestData("vnp_OrderInfo", model.OrderDescription);
			lib.AddRequestData("vnp_OrderType", model.OrderType);
			lib.AddRequestData("vnp_ReturnUrl", callbackUrl ?? string.Empty);
			lib.AddRequestData("vnp_TxnRef", orderId.ToString());

			var paymentUrl = lib.CreateRequestUrl(vnp_Url, vnp_HashSecret);
			return paymentUrl;
		}

		public PaymentResponseDto PaymentExecute(IQueryCollection queryCollection)
		{
			var vnpConfig = _configuration.GetSection("Vnpay");
			var vnp_HashSecret = vnpConfig["HashSecret"] ?? string.Empty;

			var lib = new VnPayLibrary();
			var full = lib.GetFullResponseData(queryCollection, vnp_HashSecret);
			return new PaymentResponseDto
			{
				OrderDescription = full.OrderDescription,
				TransactionId = full.TransactionId,
				OrderId = full.OrderId,
				PaymentMethod = full.PaymentMethod,
				PaymentId = full.PaymentId,
				Amount = full.Amount,
				Success = full.Success,
				Token = full.Token,
				VnPayResponseCode = full.VnPayResponseCode
			};
		}

		// Helper methods no longer needed; using VnPayLibrary for canonical signing
	}
}


