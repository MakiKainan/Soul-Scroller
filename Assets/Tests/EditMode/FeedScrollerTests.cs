using NUnit.Framework;

public class FeedScrollerTests
{
    [Test]
    public void CalculateNearestIndex_AtTop_ReturnsZero()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: 0f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(0, index);
    }

    [Test]
    public void CalculateNearestIndex_ExactlyOnSecondCard_ReturnsOne()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: 1920f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(1, index);
    }

    [Test]
    public void CalculateNearestIndex_PartwayPastThirdCard_RoundsToNearest()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: 1920f * 2.6f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(3, index);
    }

    [Test]
    public void CalculateNearestIndex_PastLastCard_ClampsToLastIndex()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: 1920f * 999f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(49, index);
    }

    [Test]
    public void CalculateNearestIndex_BeforeTop_ClampsToZero()
    {
        int index = FeedScroller.CalculateNearestIndex(contentAnchoredY: -500f, cardHeight: 1920f, cardCount: 50);
        Assert.AreEqual(0, index);
    }
}
