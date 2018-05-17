using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NBitcoin;
using Stratis.Bitcoin.Base;
using Stratis.Bitcoin.Features.BlockStore.Controllers;
using Stratis.Bitcoin.Features.BlockStore.Models;
using Stratis.Bitcoin.Interfaces;
using Stratis.Bitcoin.Tests.Common;
using Stratis.Bitcoin.Utilities.JsonErrors;
using Xunit;

namespace Stratis.Bitcoin.Features.BlockStore.Tests
{
    public class BlockStoreControllerTests
    {
        private const string ValidBlockHash = "09d889192a45ba033d4fb886d7aa62bd19b36697211b3d02ac254cf47e2326b0";
        private const string ValidTrxHash = "7416294c456639307f42740a9f0f6a1101a0c34cde3e0c055ca7ee11fee2f88c";
        private const string BlockAsHex =
            "07000000867ccd8f8b21f48e1423d2217fdfe0ea5108dcd6f3371933d584e8f250f5c6600fdf4ccef23cbdb6d81e6bde" +
            "a2f0f45aca69a35a9817590c60b5a4ce4a44d1cc30c644592060041a00000000020100000030c6445901000000000000" +
            "0000000000000000000000000000000000000000000000000000ffffffff03029423ffffffff01000000000000000000" +
            "000000000100000030c6445901795088bf033121a794ea35a11d39dbcd2495b64756e6de76d86944fdeea4ddbc020000" +
            "00484730440220096615c8fdec79ecf477cea2104859f7db98ed883f242b08fef316e3abd41a30022070d82dd743eeed" +
            "324e90cb3c168144031ba8c8b14a6af167b98253614be3d23c01ffffffff0300000000000000000000011f4a8b000000" +
            "232102e89f4f5ac02d3e5f9114253470838ee73c9ba507262ba4db7f0b3f840cf0e1d3ac40432e4a8b000000232102e8" +
            "9f4f5ac02d3e5f9114253470838ee73c9ba507262ba4db7f0b3f840cf0e1d3ac00000000463044022002efd3facb7bc9" +
            "9407d0f7c6b9c8e80898608f63f3141b06371bbd5e762dd4ab02204f1a5e8cca1a70a5b6dee55746f100042e3479c291" +
            "68dd9970c1b3147cbd6ed8";


        private const string TrxAsHex =
            "7ba2020202274786964223a2022373431363239346334353636333933303766343237343061396630663661313130316" +
            "13063333463646533653063303535636137656531316665653266383863222ca2020202276657273696f6e223a20312c" +
            "a2020202274696d65223a20313439373637393430382ca202020226c6f636b74696d65223a20302ca2020202276696e2" +
            "23a205ba2020202020207ba2020202020202020202274786964223a20226263646461346565666434343639643837366" +
            "4656536353634376236393532346364646233393164613133356561393461373231333130336266383835303739222ca" +
            "20202020202020202022766f7574223a20322ca20202020202020202022736372697074536967223a207ba2020202020" +
            "202020202020202261736d223a2022333034343032323030393636313563386664656337396563663437376365613231" +
            "303438353966376462393865643838336632343262303866656633313665336162643431613330303232303730643832" +
            "646437343365656564333234653930636233633136383134343033316261386338623134613661663136376239383235" +
            "33363134626533643233633031222ca20202020202020202020202022686578223a20223437333034343032323030393" +
            "636313563386664656337396563663437376365613231303438353966376462393865643838336632343262303866656" +
            "633313665336162643431613330303232303730643832646437343365656564333234653930636233633136383134343" +
            "0333162613863386231346136616631363762393832353336313462653364323363303122a2020202020202020207d2c" +
            "a2020202020202020202273657175656e6365223a2034323934393637323935a2020202020207da2020205d2ca202020" +
            "22766f7574223a205ba2020202020207ba2020202020202020202276616c7565223a20302ca202020202020202020226" +
            "e223a20302ca202020202020202020227363726970745075624b6579223a207ba2020202020202020202020202261736" +
            "d223a2022222ca20202020202020202020202022686578223a2022222ca2020202020202020202020202274797065223" +
            "a20226e6f6e7374616e6461726422a2020202020202020207da2020202020207d2ca2020202020207ba2020202020202" +
            "020202276616c7565223a20353938322e34342ca202020202020202020226e223a20312ca20202020202020202022736" +
            "3726970745075624b6579223a207ba2020202020202020202020202261736d223a202230326538396634663561633032" +
            "643365356639313134323533343730383338656537336339626135303732363262613464623766306233663834306366" +
            "3065316433204f505f434845434b534947222ca20202020202020202020202022686578223a202232313032653839663" +
            "466356163303264336535663931313432353334373038333865653733633962613530373236326261346462376630623" +
            "366383430636630653164336163222ca2020202020202020202020202272657153696773223a20312ca2020202020202" +
            "020202020202274797065223a20227075626b6579222ca20202020202020202020202022616464726573736573223a20" +
            "5ba20202020202020202020202020202022544a6379617445345859546b4a354459556d6f4864473945696a375733766" +
            "a475a7222a2020202020202020202020205da2020202020202020207da2020202020207d2ca2020202020207ba202020" +
            "2020202020202276616c7565223a20353938322e34352ca202020202020202020226e223a20322ca2020202020202020" +
            "20227363726970745075624b6579223a207ba2020202020202020202020202261736d223a20223032653839663466356" +
            "163303264336535663931313432353334373038333865653733633962613530373236326261346462376630623366383" +
            "43063663065316433204f505f434845434b534947222ca20202020202020202020202022686578223a20223231303265" +
            "383966346635616330326433653566393131343235333437303833386565373363396261353037323632626134646237" +
            "6630623366383430636630653164336163222ca2020202020202020202020202272657153696773223a20312ca202020" +
            "2020202020202020202274797065223a20227075626b6579222ca2020202020202020202020202261646472657373657" +
            "3223a205ba20202020202020202020202020202022544a6379617445345859546b4a354459556d6f4864473945696a37" +
            "5733766a475a7222a2020202020202020202020205da2020202020202020207da2020202020207da2020205d2ca20202" +
            "022626c6f636b68617368223a20223039643838393139326134356261303333643466623838366437616136326264313" +
            "9623336363937323131623364303261633235346366343765323332366230222ca20202022636f6e6669726d6174696f" +
            "6e73223a203430313134382ca20202022626c6f636b74696d65223a2031343937363739343038a7d";

        private const string InvalidHash = "This hash is no good";


        [Fact]
        public void GetBlock_With_null_Hash_IsInvalid()
        {
            var requestWithNoHash = new SearchByHashRequest()
            {
                Hash = null,
                OutputJson = true
            };
            var validationContext = new ValidationContext(requestWithNoHash);
            Validator.TryValidateObject(requestWithNoHash, validationContext, null, true).Should().BeFalse();
        }

        [Fact]
        public void GetBlock_With_empty_Hash_IsInvalid()
        {
            var requestWithNoHash = new SearchByHashRequest()
            {
                Hash = "",
                OutputJson = false
            };
            var validationContext = new ValidationContext(requestWithNoHash);
            Validator.TryValidateObject(requestWithNoHash, validationContext, null, true).Should().BeFalse();
        }

        [Fact]
        public void GetBlock_With_good_Hash_IsValid()
        {
            var requestWithNoHash = new SearchByHashRequest()
            {
                Hash = "some good hash",
                OutputJson = true
            };
            var validationContext = new ValidationContext(requestWithNoHash);
            Validator.TryValidateObject(requestWithNoHash, validationContext, null, true).Should().BeTrue();
        }

        [Fact]
        public void Get_Block_When_Hash_Is_Not_Found_Should_Return_Not_Found_Object_Result()
        {
            var (cache, controller) = GetControllerAndCache();

            cache.Setup(c => c.GetBlockAsync(It.IsAny<uint256>()))
                .Returns(Task.FromResult((Block)null));

            var response = controller.GetBlockAsync(new SearchByHashRequest()
            { Hash = ValidBlockHash, OutputJson = true });

            response.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundObjectResult = (NotFoundObjectResult)response.Result;
            notFoundObjectResult.StatusCode.Should().Be(404);
            notFoundObjectResult.Value.Should().Be("Block not found");
        }

        [Fact]
        public void Get_Block_When_Hash_Is_Invalid_Should_Error_With_Explanation()
        {
            var (cache, controller) = GetControllerAndCache();

            var response = controller.GetBlockAsync(new SearchByHashRequest()
            { Hash = InvalidHash, OutputJson = true });

            response.Result.Should().BeOfType<ErrorResult>();
            var notFoundObjectResult = (ErrorResult)response.Result;
            notFoundObjectResult.StatusCode.Should().Be(400);
            ((ErrorResponse)notFoundObjectResult.Value).Errors[0]
                .Description.Should().Contain("Invalid Hex String");
        }

        [Fact]
        public void Get_Block_When_Block_Is_Found_And_Requesting_JsonOuput()
        {
            var (cache, controller) = GetControllerAndCache();

            cache.Setup(c => c.GetBlockAsync(It.IsAny<uint256>()))
                .Returns(Task.FromResult(Block.Parse(BlockAsHex, Network.StratisTest)));

            var response = controller.GetBlockAsync(new SearchByHashRequest()
                {Hash = ValidBlockHash, OutputJson = true});

            response.Result.Should().BeOfType<JsonResult>();
            var result = (JsonResult) response.Result;

            result.Value.Should().BeOfType<Models.BlockModel>();
            ((BlockModel) result.Value).Hash.Should().Be(ValidBlockHash);
            ((BlockModel) result.Value).MerkleRoot.Should()
                .Be("ccd1444acea4b5600c5917985aa369ca5af4f0a2de6b1ed8b6bd3cf2ce4cdf0f");
        }

        [Fact]
        public void Get_Block_When_Block_Is_Found_And_Requesting_RawOuput()
        {
                var (cache, controller) = GetControllerAndCache();

                cache.Setup(c => c.GetBlockAsync(It.IsAny<uint256>()))
                    .Returns(Task.FromResult(Block.Parse(BlockAsHex, Network.StratisTest)));

                var response = controller.GetBlockAsync(new SearchByHashRequest()
                { Hash = ValidBlockHash, OutputJson = false });

                response.Result.Should().BeOfType<JsonResult>();
                var result = (JsonResult)response.Result;
                ((Block)(result.Value)).ToHex(Network.StratisTest).Should().Be(BlockAsHex); 
        }

        [Fact]
        public async Task GetRawTransactionAsync_TransactionInvalidHexLength_ReturnsError()
        {
            var (cache, controller) = GetControllerAndCache();

            GetRawTransactionRequest request = new GetRawTransactionRequest
            {
                txid = InvalidHash,
                verbose = 1,
                OutputJson = true
            };
            var response = controller?.GetRawTransactionAsync(request);
            response.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundObjectResult = (NotFoundObjectResult)response.Result;
            notFoundObjectResult.StatusCode.Should().Be(404);
            notFoundObjectResult.Value.Should().Be("Invalid Hex String");
        }

        [Fact]
        public async Task GetRawTransactionAsync_TransactionNull_ReturnsError()
        {
            var (cache, controller) = GetControllerAndCache();

            GetRawTransactionRequest request = new GetRawTransactionRequest
            {
                txid = null,
                verbose = 1,
                OutputJson = true
            };
            var response = controller?.GetRawTransactionAsync(request);
            response.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundObjectResult = (NotFoundObjectResult)response.Result;
            notFoundObjectResult.StatusCode.Should().Be(404);
            notFoundObjectResult.Value.Should().Be("Invalid Hex String");
        }

        [Fact]
        public async Task GetRawTransactionAsync_TransactionCannotBeFound_ReturnsException()
        {
            var (cache, controller) = GetControllerAndCache();

            GetRawTransactionRequest request = new GetRawTransactionRequest
            {
                txid = "7416294c456639307f42740a9f0f6a1101a0c34cde3e0c055ca7ee11fee2eeec",
                verbose = 1,
                OutputJson = true
            };
            var response = controller?.GetRawTransactionAsync(request);
            response.Result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundObjectResult = (NotFoundObjectResult)response.Result;
            notFoundObjectResult.StatusCode.Should().Be(404);
            notFoundObjectResult.Value.Should().Be("Transaction not found");
        }

        [Fact]
        public async Task GetRawTransactionAsync_Good_Hash_and_Requests_JSONOutput()
        {

        }
            
        private static (Mock<IBlockRepository> repository, BlockStoreController controller) GetControllerAndRepository()
        {
            Mock<ILoggerFactory> logger = new Mock<ILoggerFactory>();
            Mock<IBlockStoreCache> cache = new Mock<IBlockStoreCache>();
            Mock<ConcurrentChain> chain = new Mock<ConcurrentChain>();
            Mock<IChainState> chainState = new Mock<IChainState>();
            Mock<IBlockRepository> repository = new Mock<IBlockRepository>();

            logger.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>);

            repository.Setup(c => c.GetTrxAsync(It.IsAny<uint256>()))
                .Returns(Task.FromResult(Transaction.Parse(TrxAsHex)));

            var controller = new BlockStoreController(
                logger.Object,
                cache.Object,
                repository.Object,
                Network.StratisTest,
                chain.Object,
                chainState.Object
                );
            return (repository, controller);
        }


        private static (Mock<IBlockStoreCache> cache, BlockStoreController controller) GetControllerAndCache()
        {
            Mock<ILoggerFactory> logger = new Mock<ILoggerFactory>();
            Mock<IBlockStoreCache> cache = new Mock<IBlockStoreCache>();
            Mock<ConcurrentChain> chain = new Mock<ConcurrentChain>();
            Mock<IChainState> chainState = new Mock<IChainState>();
            Mock<IBlockRepository> repository = new Mock<IBlockRepository>();

            logger.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>);

            var controller = new BlockStoreController(
                logger.Object,
                cache.Object,
                repository.Object,
                Network.StratisTest,
                chain.Object,
                chainState.Object
                );
  
            return (cache, controller);
        }
    }
}