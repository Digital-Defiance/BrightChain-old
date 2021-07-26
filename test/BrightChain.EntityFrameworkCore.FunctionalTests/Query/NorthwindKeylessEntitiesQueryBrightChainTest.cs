// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace BrightChain.EntityFrameworkCore.Query
{
    public class NorthwindKeylessEntitiesQueryBrightChainTest : NorthwindKeylessEntitiesQueryTestBase<
        NorthwindQueryBrightChainFixture<NoopModelCustomizer>>
    {
        public NorthwindKeylessEntitiesQueryBrightChainTest(
            NorthwindQueryBrightChainFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            this.ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory]
        public override async Task KeylessEntity_simple(bool async)
        {
            await base.KeylessEntity_simple(async);

            this.AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory]
        public override async Task KeylessEntity_where_simple(bool async)
        {
            await base.KeylessEntity_where_simple(async);

            this.AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalFact] // views are not supported
        public override void KeylessEntity_by_database_view()
        {
        }

        public override void Entity_mapped_to_view_on_right_side_of_join()
        {
        }

        [ConditionalFact(Skip = "See issue#17246")]
        public override void Auto_initialized_view_set()
        {
            base.Auto_initialized_view_set();
        }

        [ConditionalFact(Skip = "issue #17246")] // collection support
        public override void KeylessEntity_with_nav_defining_query()
        {
            base.KeylessEntity_with_nav_defining_query();

            this.AssertSql(
                @"");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task KeylessEntity_with_mixed_tracking(bool async)
        {
            await this.AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().Where(ct => ct.City == "London")
                      from o in ss.Set<OrderQuery>().Where(ov => ov.CustomerID == c.CustomerID)
                      select new { c, o },
                elementSorter: e => (e.c.CustomerID, e.o.CustomerID),
                elementAsserter: (e, a) =>
                {
                    this.AssertEqual(e.c, a.c);
                    this.AssertEqual(e.o, a.o);
                });

            this.AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""City""] = ""London""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override Task KeylessEntity_with_included_nav(bool async)
        {
            return base.KeylessEntity_with_included_nav(async);
        }

        public override async Task KeylessEntity_with_defining_query(bool async)
        {
            await base.KeylessEntity_with_defining_query(async);

            this.AssertSql(
                @"SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Order"") AND (c[""CustomerID""] = ""ALFKI""))");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task KeylessEntity_with_defining_query_and_correlated_collection(bool async)
        {
            await base.KeylessEntity_with_defining_query_and_correlated_collection(async);

            this.AssertSql(
                @"SELECT c
FROM root c
WHERE (c[""Discriminator""] = ""Customer"")");
        }

        [ConditionalTheory(Skip = "issue#17314")] // left join translation
        public override async Task KeylessEntity_select_where_navigation(bool async)
        {
            await base.KeylessEntity_select_where_navigation(async);

            this.AssertSql(@"");
        }

        [ConditionalTheory(Skip = "issue#17314")] // left join translation
        public override async Task KeylessEntity_select_where_navigation_multi_level(bool async)
        {
            await this.AssertQuery(
                async,
                ss => from ov in ss.Set<OrderQuery>().Where(o => o.CustomerID == "ALFKI")
                      where ov.Customer.Orders.Any()
                      select ov);

            this.AssertSql(@"");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task KeylessEntity_groupby(bool async)
        {
            await base.KeylessEntity_groupby(async);

            this.AssertSql(@"");
        }

        [ConditionalTheory(Skip = "Issue #17246")]
        public override async Task Collection_correlated_with_keyless_entity_in_predicate_works(bool async)
        {
            await base.Collection_correlated_with_keyless_entity_in_predicate_works(async);

            this.AssertSql(@"");
        }

        private void AssertSql(params string[] expected)
        {
            this.Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
        }

        protected override void ClearLog()
        {
            this.Fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
