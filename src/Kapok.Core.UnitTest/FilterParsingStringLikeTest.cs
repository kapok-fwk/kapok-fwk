using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq.Expressions;
using Kapok.BusinessLayer.FilterParsing;
using Xunit;

namespace Kapok.Core.UnitTest;

public class FilterParsingStringLikeTest
{
    [Fact]
    public void LikeTransformationTest()
    {
        Assert.Equal("Hello World!", ExpressionHelper.TransformLikeStringToSqlLike("Hello World!"));
        Assert.Equal("%World!", ExpressionHelper.TransformLikeStringToSqlLike("*World!"));
        Assert.Equal("Hello%", ExpressionHelper.TransformLikeStringToSqlLike("Hello*"));
        Assert.Equal("Hello%!", ExpressionHelper.TransformLikeStringToSqlLike("Hello*!"));
        Assert.Equal("Hello[%]!", ExpressionHelper.TransformLikeStringToSqlLike("Hello%!"));
        Assert.Equal("Hello___", ExpressionHelper.TransformLikeStringToSqlLike("Hello???"));
        Assert.Equal("Hello [[]World[]]!", ExpressionHelper.TransformLikeStringToSqlLike("Hello [World]!"));
    }

    private class TestDestinationObject
    {
#pragma warning disable CS8618
        public string StringProperty { get; set; }
#pragma warning restore CS8618
    }

    private Expression ParseString(string filterString)
    {
        var fep = new FilterExpressionParser(typeof(TestDestinationObject), nameof(TestDestinationObject.StringProperty), filterString);
        return fep.Parse();
    }

    [Fact]
    public void ParseStringPropertyWithLikeOperationTest()
    {
        // default string match without like operator (*/?)
        Assert.Equal(" => .StringProperty == \"Hello World\"",
            ParseString("Hello World").Print());

        // use '*'
        Assert.Equal(@" => DbFunctions
    .Like(
        matchExpression: .StringProperty, 
        pattern: ""Hello%World"")",
            ParseString("Hello*World").Print());

        // use '*' part 2
        Assert.Equal(@" => DbFunctions
    .Like(
        matchExpression: .StringProperty, 
        pattern: ""Hello%World!"")",
            ParseString("Hello*World!").Print());

        // use '?'
        Assert.Equal(@" => DbFunctions
    .Like(
        matchExpression: .StringProperty, 
        pattern: ""Hello___ !"")",
            ParseString("Hello??? !").Print());

        // use '!' at the beginning to invert the condition
        Assert.Equal(@" => !(DbFunctions
    .Like(
        matchExpression: .StringProperty, 
        pattern: ""Hello___ !""))",
            ParseString("!Hello??? !").Print());

        // skip '%' SQL default like
        Assert.Equal(@" => .StringProperty == ""Hello%""",
            ParseString("Hello%").Print());

        // skip '_' SQL default like
        Assert.Equal(@" => .StringProperty == ""Hello___""",
            ParseString("Hello___").Print());

        // use '*' part 2, skip %/_ SQL default like
        Assert.Equal(@" => DbFunctions
    .Like(
        matchExpression: .StringProperty, 
        pattern: ""Hello%World[%][_]!"")",
            ParseString("Hello*World%_!").Print());

        // use '?', skip %/_ SQL default like
        Assert.Equal(@" => DbFunctions
    .Like(
        matchExpression: .StringProperty, 
        pattern: ""Hello___ [%][_]!"")",
            ParseString("Hello??? %_!").Print());
    }
}