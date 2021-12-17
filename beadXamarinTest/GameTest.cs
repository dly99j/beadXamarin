using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using beadXamarin.Model;
using beadXamarin.Persistence;
using System.Threading.Tasks;

namespace beadXamarinTest
{
    [TestClass]
    public class GameTest
    {
        private GameModel mGameModel;
        private GameTable mGameTable;
        private Mock<IGameDataAccess> mGameDataAccess;

        [TestInitialize]
        public void Initialize()
        {
            mGameTable = new GameTable();

            for (var i = 0; i < mGameTable.M; ++i)
                for (var j = 0; j < mGameTable.N; ++j)
                    mGameTable.TableCharRepresentation[i, j] = 'E';

            mGameTable.setFieldOnInit(0, 0, 'P');
            mGameTable.setFieldOnInit(0, 2, 'T');
            mGameTable.setFieldOnInit(2, 4, 'T');
            mGameTable.setFieldOnInit(2, 0, 'F');
            mGameTable.setFieldOnInit(4, 4, 'F');
            mGameTable.setFieldOnInit(0, 4, 'G');

            mGameDataAccess = new Mock<IGameDataAccess>();
            mGameDataAccess.Setup(mock => mock.LoadAsync(It.IsAny<string>()))
                           .Returns(() => Task.FromResult(mGameTable));

            mGameModel = new GameModel(mGameDataAccess.Object);

            mGameModel.Over += new EventHandler<GameEventArgs>(Model_GameOver);
        }

        [TestMethod]
        public async Task Start()
        {
            await mGameModel.LoadGameAsync(string.Empty);

            mGameModel.RefreshTable();

            Assert.AreEqual(0, mGameModel.Time);
            Assert.AreEqual(2, mGameModel.mGameTable.Foods.Count);
            Assert.AreEqual(1, mGameModel.mGameTable.NumOfGuards);
            Assert.AreEqual('P', mGameModel.CharTableRepresentation[0, 0]);

        }

        [TestMethod]
        public async Task StepTest()
        {
            await mGameModel.LoadGameAsync(string.Empty);

            mGameModel.PlayerStep(GameDirection.Down);
            Assert.AreEqual('P', mGameModel.CharTableRepresentation[1, 0]);
            Assert.AreEqual('E', mGameModel.CharTableRepresentation[0, 0]);
        }

        [TestMethod]
        public async Task AdvanceTimeTest()
        {
            await mGameModel.LoadGameAsync(string.Empty);

            mGameModel.AdvanceTime();
            Assert.AreEqual('G', mGameModel.CharTableRepresentation[1, 4]);
            Assert.AreEqual(1, mGameModel.Time);
        }

        [TestMethod]
        public async Task EatFood()
        {
            await mGameModel.LoadGameAsync(string.Empty);

            mGameModel.PlayerStep(GameDirection.Down);
            mGameModel.PlayerStep(GameDirection.Down);

            Assert.AreEqual('P', mGameModel.CharTableRepresentation[2, 0]);
            Assert.AreEqual(1, mGameModel.mGameTable.Foods.Count);
        }

        private void Model_GameOver(object sender, GameEventArgs e)
        {
            Assert.IsTrue(mGameModel.IsGameOver);
        }

    }
}