using DotNetEnv;
using PlaywrightTests.GraphQL;

namespace PlaywrightTests.Fixtures
{
    [TestClass]
    public class WopeeTestHooks
    {
        private static string? _projectUuid;
        private static string? _suiteUuid;
        private static string? _scenarioUuid;

        #region Assembly Hooks

        /// <summary>
        /// Initializes the test assembly by loading environment variables, 
        /// configuring the GraphQL client, and creating a new test suite in Wopee.
        /// </summary>
        /// <param name="context">Provides information about and functionality for the current test run.</param>
        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext context)
        {
            // PlaywrightTests/bin/Debug/net9.0/
            // Console.WriteLine(AppContext.BaseDirectory);
            Env.Load("./../../../.env");

            WopeeGraphQLClient.Initialize();

            _projectUuid = Environment.GetEnvironmentVariable("WOPEE_PROJECT_UUID")
                ?? "YOUR_PROJECT_UUID";

            var suiteIntegrationConfig = new { branchName = "main" };
            var suiteName = $"MyTestSuiteFromMSTest_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

            _suiteUuid = await CreateSuiteAsync(
                suiteIntegrationConfig,
                suiteName
            );
        }

        /// <summary>
        /// Cleans up after the test assembly by stopping the previously created suite.
        /// </summary>
        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (_suiteUuid != null)
            {
                await StopSuiteAsync(_suiteUuid);
            }
        }

        #endregion

        #region Class Hooks
        /// <summary>
        /// Cleans up after the test class by stopping the previously created scenario (if any).
        /// </summary>
        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            if (_scenarioUuid != null)
            {
                await StopScenarioAsync(_scenarioUuid);
            }
        }

        #endregion

        #region GraphQL Integrations

        /// <summary>
        /// Executes the createIntegrationSuite GraphQL mutation to create a new test suite in Wopee.
        /// Returns the UUID of the newly created suite.
        /// </summary>
        /// <param name="suiteIntegrationConfig">Additional suite configuration (e.g., branch name).</param>
        /// <param name="name">The name of the new test suite.</param>
        /// <returns>The UUID of the newly created test suite.</returns>
        public static async Task<string?> CreateSuiteAsync(
            object? suiteIntegrationConfig,
            string name)
        {
            var mutation = @"
                mutation CreateIntegrationSuite($projectUuid: ID!, $name: String!, $suiteIntegrationConfig: SuiteConfigInput) {
                createIntegrationSuite(projectUuid: $projectUuid, name: $name, suiteIntegrationConfig: $suiteIntegrationConfig) {
                    uuid
                    name
                }
                }
            ";

            var variables = new
            {
                projectUuid = _projectUuid,
                suiteIntegrationConfig,
                name
            };

            var response = await WopeeGraphQLClient.SendRequestAsync(mutation, variables);

            return WopeeGraphQLClient.ExtractFieldFromResponse(
                response,
                "createIntegrationSuite",
                "uuid"
            );
        }

        /// <summary>
        /// Executes the createIntegrationScenario GraphQL mutation to create a new scenario under the specified suite.
        /// Returns the UUID of the newly created scenario.
        /// </summary>
        /// <param name="suiteUuid">The UUID of the suite to which the scenario belongs.</param>
        /// <param name="name">An optional name for the new scenario.</param>
        /// <returns>The UUID of the newly created scenario.</returns>
        public static async Task<string?> CreateScenarioAsync(
            string suiteUuid,
            string? name = null)
        {
            var mutation = @"
                mutation CreateIntegrationScenario(
                    $projectUuid: ID!,
                    $suiteUuid: ID!,
                    $name: String
                ) {
                    createIntegrationScenario(
                        projectUuid: $projectUuid,
                        suiteUuid: $suiteUuid,
                        name: $name
                    ) {
                        integrationRunningStatus
                        name
                        uuid
                    }
                }
            ";

            var variables = new
            {
                projectUuid = _projectUuid,
                suiteUuid,
                name
            };

            var response = await WopeeGraphQLClient.SendRequestAsync(mutation, variables);
            return WopeeGraphQLClient.ExtractFieldFromResponse(
                response,
                "createIntegrationScenario",
                "uuid"
            );
        }

        /// <summary>
        /// Executes the createIntegrationStep GraphQL mutation to create a new test step within the specified scenario,
        /// optionally attaching a base64-encoded screenshot. Returns the ID of the newly created step.
        /// </summary>
        /// <param name="stepName">A descriptive name for the test step.</param>
        /// <param name="scenarioUuid">The UUID of the scenario under which the step is created.</param>
        /// <param name="imageBase64">A base64-encoded screenshot image (optional).</param>
        /// <returns>The ID of the newly created step.</returns>
        public static async Task<string?> CreateStepAsync(
            string stepName,
            string scenarioUuid,
            string imageBase64)
        {
            var mutation = @"
                mutation CreateIntegrationStep($input: CreateIntegrationStepInput!) {
                    createIntegrationStep(input: $input) {
                        id
                        stepName
                    }
                }
            ";

            var trackName = scenarioUuid + "-" + stepName;
            var variables = new
            {
                input = new
                {
                    stepName,
                    trackName,
                    projectUuid = _projectUuid,
                    scenarioUuid,
                    imageBase64
                }
            };

            var response = await WopeeGraphQLClient.SendRequestAsync(mutation, variables);
            return WopeeGraphQLClient.ExtractFieldFromResponse(response, "createIntegrationStep", "id");
        }

        /// <summary>
        /// Executes the stopIntegrationScenario GraphQL mutation to stop the specified scenario.
        /// This marks the scenario as complete in Wopee.
        /// </summary>
        /// <param name="scenarioUuid">The UUID of the scenario to stop.</param>
        public static async Task StopScenarioAsync(string scenarioUuid)
        {
            var mutation = @"
                mutation StopIntegrationScenario(
                    $scenarioUuid: ID!,
                    $projectUuid: ID!
                ) {
                    stopIntegrationScenario(
                        scenarioUuid: $scenarioUuid,
                        projectUuid: $projectUuid
                    ) {
                        uuid
                        name
                        integrationRunningStatus
                    }
                }
            ";

            var variables = new
            {
                scenarioUuid,
                projectUuid = _projectUuid
            };

            await WopeeGraphQLClient.SendRequestAsync(mutation, variables);
        }

        /// <summary>
        /// Executes the stopIntegrationSuite GraphQL mutation to stop the specified test suite.
        /// This serves as a placeholder if you have a mutation that finalizes suites in Wopee.
        /// </summary>
        /// <param name="suiteUuid">The UUID of the suite to stop.</param>
        public static async Task StopSuiteAsync(string suiteUuid)
        {
            // Example placeholder
            var mutation = @"
                mutation StopIntegrationSuite($suiteUuid: ID!) {
                    stopIntegrationSuite(suiteUuid: $suiteUuid) {
                        uuid
                    }
                }
            ";

            var variables = new
            {
                suiteUuid
            };

            await WopeeGraphQLClient.SendRequestAsync(mutation, variables);
        }

        #endregion

        #region Public Accessors

        /// <summary>
        /// Gets the UUID of the currently running test suite, if any.
        /// </summary>
        public static string? SuiteUuid => _suiteUuid;

        /// <summary>
        /// Gets the UUID of the currently running scenario, if any.
        /// </summary>
        public static string? ScenarioUuid => _scenarioUuid;

        #endregion
    }
}
