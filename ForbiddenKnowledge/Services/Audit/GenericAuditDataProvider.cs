using Audit.Core;
using Audit.WebApi;

using ForbiddenKnowledge.Data;
using ForbiddenKnowledge.Model.Audit;

namespace ForbiddenKnowledge.Services.Audit
{
	public class GenericAuditDataProvider : AuditDataProvider
	{
		private readonly AuditDbContext _auditDbContext;

		public GenericAuditDataProvider(AuditDbContext auditDbContext)
		{
			_auditDbContext = auditDbContext;
		}

        public override object InsertEvent(AuditEvent auditEvent)
        {
            AuditTrail _auditTrail = ConvertToAuditTrail(auditEvent);
            _auditDbContext.AuditTrails.Add(_auditTrail);
            _auditDbContext.SaveChangesAsync();
            return _auditTrail.Id;
        }

        private AuditTrail ConvertToAuditTrail(AuditEvent auditEvent)
        {
            AuditApiAction auditApiAction = auditEvent.GetWebApiAuditAction();
            string? referrerUrl;

            if (auditApiAction.Headers.ContainsKey("Referer") == true)
            {
                referrerUrl = string.Create(Math.Min(2048, auditApiAction.Headers["Referer"].Length), auditApiAction.Headers["Referer"], (span, input) => input.AsSpan(0, Math.Min(2048, input.Length)).CopyTo(span));
            }
            else
            {
                referrerUrl = null;
            }

            string? requestHeaders = string.Join(", ", auditEvent.Environment.CustomFields.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            string? responseHeaders = string.Join(", ", auditApiAction.ResponseHeaders.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

            string? truncatedRequestHeaders = requestHeaders != null ? string.Create(Math.Min(1000, requestHeaders.Length), requestHeaders, (span, input) => input.AsSpan(0, Math.Min(1000, input.Length)).CopyTo(span)) : null;
            string? truncatedResponseHeaders = responseHeaders != null ? string.Create(Math.Min(1000, responseHeaders.Length), responseHeaders, (span, input) => input.AsSpan(0, Math.Min(1000, input.Length)).CopyTo(span)) : null;

            return new AuditTrail
            {
                Timestamp = auditEvent.StartDate,
                IpAddress = auditApiAction.IpAddress,
                HttpMethod = auditApiAction.HttpMethod,
                RequestUrl = string.Create(Math.Min(2048, auditApiAction.RequestUrl.Length), auditApiAction.RequestUrl, (span, input) => input.AsSpan(0, Math.Min(2048, input.Length)).CopyTo(span)),
                ActionName = auditApiAction.ActionName != null ? string.Create(Math.Min(256, auditApiAction.ActionName.Length), auditApiAction.ActionName, (span, input) => input.AsSpan(0, Math.Min(256, input.Length)).CopyTo(span)) : null,
                ControllerName = auditApiAction.ControllerName != null ? string.Create(Math.Min(256, auditApiAction.ControllerName.Length), auditApiAction.ControllerName, (span, input) => input.AsSpan(0, Math.Min(256, input.Length)).CopyTo(span)) : null,
                ResponseStatusCode = auditApiAction.ResponseStatusCode,
                DurationMilliseconds = auditEvent.Duration,

                UserName = auditApiAction.UserName != null ? string.Create(Math.Min(256, auditApiAction.UserName.Length), auditApiAction.UserName, (span, input) => input.AsSpan(0, Math.Min(256, input.Length)).CopyTo(span)) : null,
                UserAgent = auditApiAction.Headers.ContainsKey("User-Agent") == true ? string.Create(Math.Min(512, auditApiAction.Headers["User-Agent"].Length), auditApiAction.Headers["User-Agent"], (span, input) => input.AsSpan(0, Math.Min(512, input.Length)).CopyTo(span)) : null,
                RequestBody = auditApiAction.RequestBody != null ? string.Create(Math.Min(2000, auditApiAction.RequestBody.ToString().Length), auditApiAction.RequestBody.ToString(), (span, input) => input.AsSpan(0, Math.Min(2000, input.Length)).CopyTo(span)) : null,
                ResponseBody = auditApiAction.ResponseBody != null ? string.Create(Math.Min(2000, auditApiAction.ResponseBody.ToString().Length), auditApiAction.ResponseBody.ToString(), (span, input) => input.AsSpan(0, Math.Min(2000, input.Length)).CopyTo(span)) : null,
                SessionId = string.Create(Math.Min(128, auditApiAction.TraceId.Length), auditApiAction.TraceId, (span, input) => input.AsSpan(0, Math.Min(128, input.Length)).CopyTo(span)),
                ReferrerUrl = referrerUrl,

                RequestHeaders = truncatedRequestHeaders,
                ResponseHeaders = truncatedResponseHeaders,

                Exception = auditApiAction.Exception != null ? string.Create(Math.Min(1000, auditApiAction.Exception.Length), auditApiAction.Exception, (span, input) => input.AsSpan(0, Math.Min(1000, input.Length)).CopyTo(span)) : null
            };
        }


    }
}
