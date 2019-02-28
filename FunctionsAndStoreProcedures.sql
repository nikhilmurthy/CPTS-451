use YelpDB
go

--  function definitions --

DROP FUNCTION fnDistance
go
  
--  returns the distance between two locations in meters
--  p1, p2 - latitude & longitude for location 1
--  p3, p4 - same for location 2

CREATE FUNCTION fnDistance (@p1 numeric(20,10), @p2 numeric(20,10),@p3 numeric(20,10),@p4 numeric(20,10) ) RETURNS float
 AS 
 BEGIN 
 declare @return float;
 declare @p5 geography;
  declare @p6 geography;

 set @p5 = geography::Point(@p1, @p2, 4326);
  set @p6 = geography::Point(@p3, @p4, 4326);

 set @return = @p5.STDistance(@p6)

 return @return 
 end
 go


 DROP FUNCTION fnTrend
go


-- This function returns 1 if the star rating is showing positive trend (ie each new raing is same or better than the old one)
-- and the last rating is better than the first rating; It will return 0 otherwise
-- The review table is scanned for a specific business and year; each row is compared with the previous until the end or trend becomes negative

CREATE FUNCTION fnTrend
 (
      @bid char(22),
	  @year char(4)
        )                       
returnS int
as

BEGIN


    DECLARE @SqlString varchar(80)
    Declare @P0 int -- first row valuw
    Declare @P1 int -- current value 
	Declare @P2 int -- last value
	Declare @P3 int -- total rows processed
 
 

	set @P2 = 0
	set @P3 = 0

    select @SqlString =''

    Declare @MyCursor as Cursor
          SET @MyCursor = CURSOR FAST_FORWARD 
          FOR 
		  Select stars from Review
		  where Review.business_id = @bid
		  and Year(Review_date) = @year  
		  order by review_date asc

       OPEN @MyCursor 

         FETCH NEXT FROM @MyCursor
         INTO @P1
		IF (@@FETCH_STATUS = 0)
		  set @P0 = @P1;
		  
        WHILE @@FETCH_STATUS = 0 
        BEGIN 
		if (@P1 <  @P2)
		   BEGIN
		   CLOSE @MyCursor
		   DEALLOCATE @MyCursor
		   return 0 -- @P3
		   END
		set @P3 = @P3 + 1
		set @P2 = @P1
        FETCH NEXT FROM @MyCursor INTO @P1
        END 

CLOSE @MyCursor
DEALLOCATE @MyCursor

IF (@P2 > @P0)
  RETURN 1
 
RETURN 0
END
GO


-- Queries --

 
-- example
-- select dbo.distance(47.6062095, -122.3320708,46.7312745,-117.1796158 );



drop procedure Q1
go

-- Store procedure to implement query Q1
-- Given a “business category” and the current location, finds the businesses within 10 miles of the current location
-- The result is sorted based on (i) rating (ii) review count (iii) proximity to current location.
-- p1 = category; p2 = current location (lat); p3 = current location (long)

create procedure Q1 @p1 varchar(50), @p2 numeric(20, 10), @p3 numeric(20, 10) as

BEGIN

 select * 
 from
	 (select B.Business_id, B.name, B.full_address, B.stars_avg, B.review_count,  dbo.fnDistance(@P2, @P3, B.latitude, B.longitude) as distance_meters
	  from 
		 Business B, 
		 BusinessCategory C
	  Where B.Business_id = C.business_id and C.category_name = @p1) X
 where X.distance_meters < 16093.4  -- 10 miles in meters
 order by stars_avg desc, review_count desc, distance_meters asc

END

GO

-- examples
-- Location is of the business with Business ID = '--5jkZ3-nUPZxUvtcbr8Uw',  name = George Gyros Greek Grill

-- Select '** Query Q1 Results **'
-- go

-- exec Q1 "Greek", 33.4633733188, -111.9269084930 
-- go

-- Query Q2 (part 1) --
 drop procedure Q2_1
 go

--  For each business category, find the businesses that were rated best in June 2011. 
--  use the rank function with partition/order and select the top ranking entrie(s) for each category

 create procedure Q2_1 as

 BEGIN
 
  select category_name, B.business_id, B.name, B.full_address, Y.stars_avg, Y.review_count, rank
  from
	  (select C.category_name, X.Business_id, review_count, stars_avg, rank() over (partition by category_name order by stars_avg desc) as rank
		from 
		 (select business_id, AVG(cast (stars as float)) as stars_avg, count(*) as review_count from Review R
		  where R.review_date like '2011-06%'
		  group by business_id) X,
		BusinessCategory C
		Where X.business_id = C.business_id 
	  ) Y,
	Business B
  where B.business_id = Y.business_id and Y.rank =1
  order by category_name

END

go

-- Select '** Results from Q2 (part-1) **'
-- go

-- exec Q2_1

-- go

--- Query 2 (part 2) --

--  Finds the restaurants that steadily improved their ratings during the year of 2012.
--  Makes use of the fnTrnd function to identify which business show positve trend
--  See comments for the function for more details
 
 drop procedure Q2_2
 go

create procedure Q2_2 as

BEGIN
 
select B.business_id, B.name, B.full_address, Z.review_count, Z.trend 
from
	(
		select R.business_id, count(*) as review_count, dbo.fnTrend(R.business_id, Year(review_date)) as trend
		from review R, BusinessCategory C
		where  R.business_id = C.business_id and C.category_name = 'Restaurants' and Year(review_date) = '2012' 
		group by R.business_id, Year(review_date)
	) Z,
	Business B
where B.business_id = Z.business_id and trend = 1

END
go

-- Select ' ** Results from Q2 (part 2) **'
-- go

-- exec Q2_2
-- go

--- Query 3 ---

drop procedure Q3
go

-- Given a “business”  and distance, provides competitive rankings against other businesses in all categories for that business. 
-- The result will include the following for each category: number of businesses, ranking(avg stars), ranking (%reviews with 3.0 or higher stars)
-- , ranking(weighted avg stars).    

create procedure Q3 @p1 char(24), @p3 int as

BEGIN

Select Z.cname, Z.bcount, Y.ranking_Stars, Y.ranking_positve
from
	(select * 
	from
		(select bid2, cname, RANK() over (PARTITION by cname ORDER BY rc_avgStars desc) as ranking_Stars, RANK() over (PARTITION by cname ORDER BY (rc_g3*100/rc) desc) as ranking_positve
		from
			(select
	    		bid2,
				cname,
				AVG(distancex) as distance,
				count(*) as rc,
				AVG(cast(stars as float)) as rc_avgStars,
				SUM (case when (stars > 3) then 1 else 0 END) as rc_g3

				from

				review,

				(select bid2, cname, distancex 
				from
					(select  B.business_id as bid2, dbo.distance (A.latitude, A.longitude, B.latitude, B.longitude) as distancex, C.category_name as cname
 
						from 
						business A, 
						business B, 
						businessCategory C
						where 
						A.business_id=@p1 and 
						B.business_id = C.business_id and
						C.category_name in (select category_name from BusinessCategory where business_id = @p1)
						 
						) sub
				where distancex < @p3 
				) Blist

			where 
				review.business_id = bid2
			group by 
				bid2,cname
			) final

		) X,
		business B
	where 
		X.bid2 = B.business_id and X.bid2 = @p1

	) Y,

	-- get the count of businesses for each category for the speciifed business within a certain distance
	-- need this subquery to get the counts
	  
	(select cname, count(*) as bcount 
		from
			(select dbo.distance (A.latitude, A.longitude, B.latitude, B.longitude) as distancex, C.category_name as cname
 
				from 
					business A, 
					business B, 
					BusinessCategory C 
				where 
					A.business_id=@p1 and 
					C.category_name in (select category_name from businessCategory where business_id = @p1)
					and B.business_id = C.business_id   
			) sub
		where 
		   distancex < @p3
		group by 
			cname
	 ) Z

where 
	Y.cname = Z.cname
order by 
	Y.cname 

END 

go

-- Select ' ** Results from Q3 **'
-- go

-- exec Q3 'PzOqRohWw7F7YEPBz6AubA', 10000

-- go

-- Query Q4 ---

drop procedure Q4
go

-- Given a “business Category”, a location, distance and a list of key words
-- reports the following data for businesses that has reviews that matches one or more of the keywords
-- name & address, review count and overall rating, review counts that matches key words & associated rating (avg stars). 

create procedure Q4 @p2 varchar(50), @p3 int, @p4 varchar(1024),  @p5 numeric(20, 10), @p6 numeric(20, 10) as
BEGIN


select B.name, B.full_address, B.city, B.review_count, B.stars_avg, rc, avg_rm_stars

from
    -- get the businesses that belongs to the named category and within a certain distance

	(select bid2, distancex from
        (select B.business_id as bid1, B.business_id as bid2, B.name as name2, dbo.distance (@p5, @p6, B.latitude, B.longitude) as distancex
				from
				business B,
				BusinessCategory C 
				where C.category_name = @p2 and B.business_id = C.business_id    -- and A.stars <= B.stars 
		) sub
	where distancex < @p3
	) J,

	-- find the businesses that conatins the keywords in reviews and get the avg stars for those reviews
	(
	select  r.business_id as id, count(*) as rc, AVG(cast (stars as float)) as avg_rm_stars 
	from review r
	where CONTAINS ( r.review_text,  @p4)  
	group by r.business_id
	) X,

 business B
 where x.id = J.bid2 and x.id = B.business_id and bid2 = B.business_id
 order by avg_rm_stars desc
 END

 go

-- Select ' ** Results from Q4 **'
-- go

-- exec Q4 'Food', 5000, 'chicken', 33.4633733188, -111.9269084930 

-- go

--- Query Q5 ---

drop procedure Q5
go

-- For each Zipcode, provide the count of restaurants for the 10 most popular international category 
-- (Ex: Chinese, French, Greek, Indian, Irish, Italian, Korean, Mexican, Thai and Vietnamese 
-- Theresults is ordered based on the total count of restuarents serving the popular international cousine
-- Makes use of the pivot function to make column value into a column.


create procedure Q5 as

BEGIN

 select TOP 20  zipcode, Chinese, French, German, Greek, Indian, Irish, Italian, Korean, Mexican, Thai, Vietnamese,
               (Chinese + French + German + Greek + Indian + Irish + Italian + Korean + Mexican +Thai + Vietnamese) as Total 
  from
	 ( select   category_name, substring(full_address, len(full_address) -4, 5) as zipcode 
	   from 
		  (select B.* from business B, BusinessCategory C where B.business_id = C.business_id and C.category_name = 'Restaurants') X,
		  BusinessCategory Y
       where X.business_id = Y.business_id) Z
  pivot (count(category_name) for category_name in (Chinese, French, German, Greek, Indian, Irish, Italian, Korean, Mexican, Thai, Vietnamese)) pvt
  order by Total desc

END

go

--Select ' ** Results from Q5 **'
--go

--exec Q5
--go

