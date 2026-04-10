using System;
using MongoDB.Bson;

namespace ET.Test
{
    public class Test_UnityBridge_DeferredWorkflow_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await ETTask.CompletedTask;

            CompileResponse compileResponse = CompileResponse.Create();
            if (UnityBridgeResponseHelper.IsDeferredResponse(compileResponse))
            {
                return Fail(1, "normal response should not be treated as deferred");
            }

            IResponse deferredResponse = UnityBridgeResponseHelper.CreateDeferredResponse();
            if (!UnityBridgeResponseHelper.IsDeferredResponse(deferredResponse))
            {
                return Fail(2, "deferred response should be recognized");
            }

            Compile compile = Compile.Create();
            compile.RpcId = 1001;
            string commandJson = UnityBridgeMongoJsonHelper.ToCommandJson(compile);
            if (string.IsNullOrWhiteSpace(commandJson))
            {
                return Fail(3, "command json should not be empty");
            }

            BsonDocument commandDocument = BsonDocument.Parse(commandJson);
            if (!commandDocument.TryGetValue("_t", out BsonValue commandTypeValue))
            {
                return Fail(4, "command json should include _t discriminator");
            }

            if (commandTypeValue.AsString != typeof(Compile).FullName)
            {
                return Fail(5, "command json should persist full command type");
            }

            UnityBridgeDeferredCommandState pendingState = new()
            {
                CommandJson = commandJson,
                RpcId = 1001,
                IdempotencyKey = "idem",
                TimeoutMs = 12345,
                StartedAt = 67890
            };

            string pendingJson = UnityBridgeMongoJsonHelper.ToJson(pendingState);
            if (pendingJson.IndexOf("ProcessingPath", StringComparison.Ordinal) >= 0)
            {
                return Fail(6, "pending state should not persist processing path");
            }

            UnityBridgeDeferredCommandState roundTripState = MongoHelper.FromJson<UnityBridgeDeferredCommandState>(pendingJson);
            if (roundTripState == null)
            {
                return Fail(7, "pending state should round-trip");
            }

            if (roundTripState.RpcId != pendingState.RpcId ||
                roundTripState.TimeoutMs != pendingState.TimeoutMs ||
                roundTripState.StartedAt != pendingState.StartedAt)
            {
                return Fail(8, "pending state should keep scalar fields");
            }

            if (roundTripState.CommandJson != pendingState.CommandJson ||
                roundTripState.IdempotencyKey != pendingState.IdempotencyKey)
            {
                return Fail(9, "pending state should keep payload fields");
            }

            ErrorResponse errorResponse = UnityBridgeResponseHelper.CreateErrorResponse(
                2001,
                nameof(Compile),
                UnityBridgeErrorCode.HandlerFail,
                "boom");
            if (errorResponse.RpcId != 2001 ||
                errorResponse.Command != nameof(Compile) ||
                errorResponse.Error != UnityBridgeErrorCode.HandlerFail ||
                errorResponse.Message != "boom")
            {
                return Fail(10, "error response helper should preserve response fields");
            }

            return ErrorCode.ERR_Success;
        }

        private static int Fail(int code, string message)
        {
            Log.Console(message);
            return code;
        }

    }
}
