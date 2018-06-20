using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Collections.Generic;

namespace MyCasesAction
{
    public class MyCasesAction : CodeActivity
    {
        [Output("Count of Cases")]
        public OutArgument<int> CaseCount { get; set; }

        [Output("Name of Cases")]
        public OutArgument<string> CaseNames { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            // Getting OrganizationService from Context
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var orgService = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            // fetchXml to retrieve cases for current user
            var fetchXmlCurrentUserCases = @"
            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
              <entity name='incident'>
                <attribute name='title' />
                <filter type='and'>
                  <condition attribute='ownerid' operator='eq-userid' />
                </filter>
              </entity>
            </fetch>";

            // Retrieving cases using fetchXml
            var cases = orgService.RetrieveMultiple(
                new FetchExpression(
                    fetchXmlCurrentUserCases));

            // Null check
            if (cases == null || cases?.Entities == null) return;

            // Adding Case names/titles to list
            var allCases = new List<string>();
            foreach (var cs in cases.Entities)
                allCases.Add(cs["title"].ToString());

            // Set Count of cases to CaseCount
            this.CaseCount.Set(context, cases.Entities.Count);

            // Set comma separated Case Titles to CaseNames
            this.CaseNames.Set(context, string.Join(",", allCases.ToArray()));
        }
    }
}