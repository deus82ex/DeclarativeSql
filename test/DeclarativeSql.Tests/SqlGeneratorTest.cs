﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DeclarativeSql.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace DeclarativeSql.Tests
{
    [TestClass]
    public class SqlGeneratorTest
    {
        //--- とりあえず決め打ちで確認
        protected DbProvider DbProvider { get; } = DbProvider.SqlServer;


        #region Count
        [TestMethod]
        public void Count文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateCount(typeof(Person));
            var actual2 = this.DbProvider.Sql.CreateCount<Person>();
            var expect = "select count(*) as Count from dbo.Person";
            actual1.Is(expect);
            actual2.Is(expect);
        }
        #endregion


        #region Select
        [TestMethod]
        public void 全列のSelect文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateSelect(typeof(Person));
            var actual2 = this.DbProvider.Sql.CreateSelect<Person>();
            var expect =
@"select
    Id as Id,
    名前 as Name,
    Age as Age,
    HasChildren as HasChildren
from dbo.Person";
            actual1.Is(expect);
            actual2.Is(expect);
        }


        [TestMethod]
        public void 特定1列のSelect文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateSelect<Person>(x => x.Name);
            var actual2 = this.DbProvider.Sql.CreateSelect<Person>(x => new { x.Name });
            var expect =
@"select
    名前 as Name
from dbo.Person";
            actual1.Is(expect);
            actual2.Is(expect);
        }


        [TestMethod]
        public void 特定2列のSelect文生成()
        {
            var actual = this.DbProvider.Sql.CreateSelect<Person>(x => new { x.Name, x.Age });
            var expect =
@"select
    名前 as Name,
    Age as Age
from dbo.Person";
            actual.Is(expect);
        }
        #endregion


        #region Insert
        [TestMethod]
        public void シーケンスを利用するInsert文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateInsert(typeof(Person));
            var actual2 = this.DbProvider.Sql.CreateInsert<Person>();
            var expect =
@"insert into dbo.Person
(
    名前,
    Age,
    HasChildren
)
values
(
    @Name,
    next value for dbo.AgeSeq,
    @HasChildren
)";
            actual1.Is(expect);
            actual2.Is(expect);
        }


        [TestMethod]
        public void シーケンスを利用しないInsert文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateInsert(typeof(Person), false);
            var actual2 = this.DbProvider.Sql.CreateInsert<Person>(false);
            var expect =
@"insert into dbo.Person
(
    名前,
    Age,
    HasChildren
)
values
(
    @Name,
    @Age,
    @HasChildren
)";
            actual1.Is(expect);
            actual2.Is(expect);
        }


        [TestMethod]
        public void IDを設定するInsert文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateInsert(typeof(Person), setIdentity: true);
            var actual2 = this.DbProvider.Sql.CreateInsert<Person>(setIdentity: true);
            var expect =
@"insert into dbo.Person
(
    Id,
    名前,
    Age,
    HasChildren
)
values
(
    @Id,
    @Name,
    next value for dbo.AgeSeq,
    @HasChildren
)";
            actual1.Is(expect);
            actual2.Is(expect);
        }
        #endregion


        #region Update
        [TestMethod]
        public void 全列のUpdate文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateUpdate(typeof(Person));
            var actual2 = this.DbProvider.Sql.CreateUpdate<Person>();
            var expect =
@"update dbo.Person
set
    名前 = @Name,
    Age = @Age,
    HasChildren = @HasChildren";
            actual1.Is(expect);
            actual2.Is(expect);
        }


        [TestMethod]
        public void 特定1列のUpdate文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateUpdate(typeof(Person), new [] { "Name" });
            var actual2 = this.DbProvider.Sql.CreateUpdate<Person>(x => x.Name);
            var actual3 = this.DbProvider.Sql.CreateUpdate<Person>(x => new { x.Name });
            var expect =
@"update dbo.Person
set
    名前 = @Name";
            actual1.Is(expect);
            actual2.Is(expect);
            actual3.Is(expect);
        }


        [TestMethod]
        public void 特定2列のUpdate文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateUpdate(typeof(Person), new [] { "Name", "Age" });
            var actual2 = this.DbProvider.Sql.CreateUpdate<Person>(x => new { x.Name, x.Age });
            var expect =
@"update dbo.Person
set
    名前 = @Name,
    Age = @Age";
            actual1.Is(expect);
            actual2.Is(expect);
        }


        [TestMethod]
        public void IDを設定するUpdate文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateUpdate(typeof(Person), setIdentity: true);
            var actual2 = this.DbProvider.Sql.CreateUpdate<Person>(setIdentity: true);
            var expect =
@"update dbo.Person
set
    Id = @Id,
    名前 = @Name,
    Age = @Age,
    HasChildren = @HasChildren";
            actual1.Is(expect);
            actual2.Is(expect);
        }
        #endregion


        #region Delete
        [TestMethod]
        public void Delete文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateDelete(typeof(Person));
            var actual2 = this.DbProvider.Sql.CreateDelete<Person>();
            var expect = "delete from dbo.Person";
            actual1.Is(expect);
            actual2.Is(expect);
        }
        #endregion


        #region Truncate
        [TestMethod]
        public void Truncate文生成()
        {
            var actual1 = this.DbProvider.Sql.CreateTruncate(typeof(Person));
            var actual2 = this.DbProvider.Sql.CreateTruncate<Person>();
            var expect = "truncate table dbo.Person";
            actual1.Is(expect);
            actual2.Is(expect);
        }
        #endregion


        #region Where
        [TestMethod]
        public void 等しい()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id == 1);

            var expectStatement = "Id = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 等しくない()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id != 1);

            var expectStatement = "Id <> @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void より大きい()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id > 1);

            var expectStatement = "Id > @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void より小さい()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id < 1);

            var expectStatement = "Id < @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 以上()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id >= 1);

            var expectStatement = "Id >= @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 以下()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id <= 1);

            var expectStatement = "Id <= @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void Null()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Name == null);

            var expectStatement = "名前 is null";
            IDictionary<string, object> expectParameter = new ExpandoObject();

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 非Null()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Name != null);

            var expectStatement = "名前 is not null";
            IDictionary<string, object> expectParameter = new ExpandoObject();

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void And()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id > 1 && x.Name == "xin9le");

            var expectStatement = "Id > @p0 and 名前 = @p1";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);
            expectParameter.Add("p1", "xin9le");

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void Or()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id > 1 || x.Name == "xin9le");

            var expectStatement = "Id > @p0 or 名前 = @p1";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);
            expectParameter.Add("p1", "xin9le");

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void AndOr1()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id > 1 && x.Name == "xin9le" || x.Age <= 30);

            var expectStatement = "(Id > @p0 and 名前 = @p1) or Age <= @p2";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);
            expectParameter.Add("p1", "xin9le");
            expectParameter.Add("p2", 30);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void AndOr2()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id > 1 && (x.Name == "xin9le" || x.Age <= 30));

            var expectStatement = "Id > @p0 and (名前 = @p1 or Age <= @p2)";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);
            expectParameter.Add("p1", "xin9le");
            expectParameter.Add("p2", 30);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void AndOr3()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id > 1 || x.Name == "xin9le" && x.Age <= 30);

            var expectStatement = "Id > @p0 or (名前 = @p1 and Age <= @p2)";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);
            expectParameter.Add("p1", "xin9le");
            expectParameter.Add("p2", 30);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void AndOr4()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => (x.Id > 1 || x.Name == "xin9le") && x.Age <= 30);

            var expectStatement = "(Id > @p0 or 名前 = @p1) and Age <= @p2";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", 1);
            expectParameter.Add("p1", "xin9le");
            expectParameter.Add("p2", 30);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void Contains()
        {
            var value = Enumerable.Range(0, 3).Cast<object>().ToArray();
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => value.Contains(x.Id));

            var expectStatement = "Id in @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", value);

            actual.Parameter.IsStructuralEqual(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void Contains1000件超()
        {
            var value1 = Enumerable.Range(0, 1000).Cast<object>().ToArray();
            var value2 = Enumerable.Range(1000, 234).Cast<object>().ToArray();
            var value  = value1.Concat(value2);
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => value.Contains(x.Id));

            var expectStatement = "Id in @p0 or Id in @p1";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", value2);
            expectParameter.Add("p1", value1);

            actual.Parameter.IsStructuralEqual(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 右辺が変数()
        {
            var id = 1;
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id == id);

            var expectStatement = "Id = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", id);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 右辺がコンストラクタ()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Name == new string('a', 3));

            var expectStatement = "名前 = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", "aaa");

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }

/*
        [TestMethod]
        public void 右辺が配列()
        {}
*/

        [TestMethod]
        public void 右辺がメソッド()
        {
            var some = new AccessorProvider();
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Name == some.InstanceMethod());

            var expectStatement = "名前 = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", some.InstanceMethod());

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 右辺がラムダ式()
        {
            Func<int, string> getName = x => x.ToString();
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Name == getName(123));

            var expectStatement = "名前 = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", "123");

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 右辺がプロパティ()
        {
            var some = new AccessorProvider();
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Age == some.InstanceProperty);

            var expectStatement = "Age = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", some.InstanceProperty);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 右辺がインデクサ()
        {
            var ids = new [] { 1, 2, 3 };
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Id == ids[0]);

            var expectStatement = "Id = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", ids[0]);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 右辺が静的メソッド()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Name == AccessorProvider.StaticMethod());

            var expectStatement = "名前 = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", AccessorProvider.StaticMethod());

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void 右辺が静的プロパティ()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.Age == AccessorProvider.StaticProperty);

            var expectStatement = "Age = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", AccessorProvider.StaticProperty);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void Boolean()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.HasChildren);

            var expectStatement = "HasChildren = @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", true);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void InverseBoolean()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => !x.HasChildren);

            var expectStatement = "HasChildren <> @p0";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", true);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }


        [TestMethod]
        public void BooleanAndOr()
        {
            var actual = this.DbProvider.Sql.CreateWhere<Person>(x => x.HasChildren == true || x.Id != 0 || x.Name == "xin9le" && !x.HasChildren);

            var expectStatement = "HasChildren = @p0 or Id <> @p1 or (名前 = @p2 and HasChildren <> @p3)";
            IDictionary<string, object> expectParameter = new ExpandoObject();
            expectParameter.Add("p0", true);
            expectParameter.Add("p1", 0);
            expectParameter.Add("p2", "xin9le");
            expectParameter.Add("p3", true);

            actual.Parameter.Is(expectParameter);
            actual.Statement.Is(expectStatement);
        }
        #endregion
    }
}