IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_getcarveouttranextdata]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_getcarveouttranextdata]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	
	Created by Manmit/Aritra On 24th Jan 2021 Issue Id - FITEK049_22821 -- A new extract "Carveout Transaction	Extract" has ben introduced.
	Modified by Aritra on 25th Jan Issue Id - FITEK049_22821 -- a space added in 'Re-allocation '.Eligible spelling corrected.
    Modified by manmit on 27th Jan Issue Id - FITEK049_22821 -- further changes...
    Modified by manmit on 28th Jan Issue Id - FITEK049_22821 -- further changes due to problem in [Pre incentive carveout amount] column data.
    Modified by manmit on 31st Jan Issue Id - FITEK049_22821 -- data is not getting populated after any booked co which has no re-allocation..
	Modified by Pooja On 17th Mar 2022 Issue Id - FITEK049_22915 -- three new columns Investment Date, Notice Period & Notice Period Start Date has been introduced.
	Modified by pooja  on 23rd Mar Issue Id - FITEK049_22915 -- 1.There is an error while no reallocation is present -- Resolved 2.for Prior BP EP,new logic has been introduced.
	Modified by Manmit on 10th may 2022 Issue Id - FITEK049_22915 -- To get prior bp ep data,group order will be always 1..
	Modified by Manmit on 5th July Issue Id-- FITEK049_22915 -- column with same name in a temporary table removed.further changes..
*/

--exec sp_getcarveouttranextdata 11,20210201,1,1

CREATE PROCEDURE [dbo].[sp_getcarveouttranextdata] 
@mAcctId as int,
@mBpid as int,
@moption as int = 1, ---------------1.populate grid data 2.Extract generation
@muserid as int


AS
begin


		if @moption = 1
		begin
			Select distinct X.* from
			(
			select 0 as sel,allocclassid,allocclassnm
			from CarveOutTran c
			inner join allocclass a 
				on a.AcctId = c.AcctId and a.AllocClassId = c.FromAllocClassId and a.IsSidePckt =1
			where c.acctid = @mAcctId  and c.BrkprdId =@mBpid 
			UNION
			select 0 as sel,allocclassid,allocclassnm
			from CarveOutTran c
			inner join allocclass a 
				on a.AcctId = c.AcctId and a.AllocClassId = c.ToAllocClassId  and a.IsSidePckt =1
			where c.acctid = @mAcctId  and c.BrkprdId =@mBpid 
			) X
			order by x.AllocClassNm 

select 6
		end
		if @moption = 2
		begin
			declare @cols NVARCHAR(MAX),@query  AS NVARCHAR(MAX),@colsnew Nvarchar(max),@colstot Nvarchar(max),@colclassnm Nvarchar(max)
			declare @mclsid int,@mclsnm varchar(100) ,@realloc NVARCHAR(MAX)
			Create table #COTRANFINAL
			(
			[Partner Id]							varchar(30)		NULL,	
			[Partner Name]							varchar(100)	NULL,	
			[Tranche Name]							varchar(30)		NULL,	
			[Source Allocation Class]				varchar(100)	NULL,	
			[Target Allocation Class]				varchar(100)	NULL,	
			[Special Investment Option]				varchar(100)	NULL,	
			[Transfer Distribution Rule]			varchar(100)	NULL,	
			[Group Order]							Int				NULL,	
			[Carveout Type]							varchar(30)		NULL,	
			[Close Source]							varchar(10)		NULL,	
			[No Source]								varchar(10)		NULL,
			[Pseudo Partner]						varchar(10)		NULL,
			[Side Pocket Eligible]					varchar(10)		NULL,	
			[Investment Date]						varchar(10)		Null,
			[Notice Period]							int				Null,
			[Notice Period start Date]				varchar(10)		Null,
			[Source Base Capital For Carveout]		NUMERIC(26,2)	NULL,	
			[Remaining Capacity Available]			NUMERIC(26,2)	NULL,	
			[Initial Allocation]					NUMERIC(26,2)	NULL,	
			[Pre incentive carveout amount]			NUMERIC(26,2)	NULL,	
			[Percentage Of Transfer]				NUMERIC(28,17)	NULL,	
			--[Percentage Of Transfer]				VARCHAR(30) 	NULL,	
			[Target Allocation Class Book Capital]	NUMERIC(26,2)	NULL,
			[Reallocation]							VARCHAR(50)		NULL,
			[Reallocationamt]						VARCHAR(50)		NULL,
			[frallocid]								INT				NULL,
			[toallocid]								INT				NULL

			)
			
			Create table #COTRANFINAL1
			(
			[Partner Id]							varchar(30)		NULL,	
			[Partner Name]							varchar(100)	NULL,	
			[Tranche Name]							varchar(30)		NULL,	
			[Source Allocation Class]				varchar(100)	NULL,	
			[Target Allocation Class]				varchar(100)	NULL,	
			[Special Investment Option]				varchar(100)	NULL,	
			[Transfer Distribution Rule]			varchar(100)	NULL,	
			[Group Order]							Int				NULL,	
			[Carveout Type]							varchar(30)		NULL,	
			[Close Source]							varchar(10)		NULL,	
			[No Source]								varchar(10)		NULL,
			[Pseudo Partner]						varchar(10)		NULL,
			[Side Pocket Eligible]					varchar(10)		NULL,	
			[Investment Date]						varchar(10)		Null,
			[Notice Period]							int				Null,
			[Notice Period start Date]				varchar(10)		Null,
			[Source Base Capital For Carveout]		NUMERIC(26,2)	NULL,	
			[Remaining Capacity Available]			NUMERIC(26,2)	NULL,	
			[Initial Allocation]					NUMERIC(26,2)	NULL,	
			[Pre incentive carveout amount]			NUMERIC(26,2)	NULL,	
			[Percentage Of Transfer]				NUMERIC(28,17)	NULL,		
			[Target Allocation Class Book Capital]	NUMERIC(26,2)	NULL,
			[SORT]									varchar(100)	NULL
			)

			----------------------MAIN CARVEOUT TRANSACTION DATA-----------------------------------------
			
			Select distinct X.*,@muserid as userid,@mBpid as brkprdid into #MAINCODATA  from
			(
			select a.acctid,c.TranId ,a.allocclassid as frallocid,a1.allocclassid as toallocid,
			a.AllocClassNm as fromallocclassnm,a1.allocclassnm as ToAllocclassnm,
			case when c.TrnchOption = 0  then 'Create Special Investment Account'
				 when c.TrnchOption = 1  then 'Follow-Up Investment - Exclude New Partners'
				 when c.TrnchOption = 2  then 'Follow-Up Investment - Include New Partners' 
			else 'Create Special Investment Account' end AS spclinvtmentopt,
			Case Cd.CdId When 494 Then 'By Source Capital %' When 495 Then 'By Target Capital %' 
			When 734 Then 'BY Other Source EP %' When 741 Then 'BY Other Source EP (Partner Level) %' 
			When 743 Then 'By Source EP (Partner Level) %' 
			When 744 Then 'By Target Capacity EP %' 
			when 1150 Then 'By Profile EP %'
			when 1287 Then 'By Source Capacity EP %'
			End  as DistributionRule 
			,c.GroupOrder,case when IsOpening = 1 then 'Opening Carveout' Else 'Closing Carveout' End As Carveouttyp 
			,case when c.IsShutDownSP =1 then 'Yes' Else 'No' End As Closesource
			,case when c.NoSource =1 then 'Yes' Else 'No' End As Nosource
			,case when c.IsIncludePseudo =1 then 'Yes' Else 'No' End As pseudoprtnr
			,case when c.PrtnrSelectionType  =2 then 'Yes' Else 'No' End As spelligible
			,Convert (varchar(10),CreationDt,101) as InvestmentDt
			,c.PriorEPBrkprd
			,c.IsOpening
			from CarveOutTran c WITH (NOLOCK)
			inner join allocclass a WITH (NOLOCK) 
				on a.AcctId = c.AcctId and a.AllocClassId = c.FromAllocClassId and a.IsSidePckt =1
			inner join sidepcktallocclass_stg s WITH (NOLOCK)
				on a.AcctId = s.AcctId and a.AllocClassId =s.allocclassid and s.userid = @muserid and s.isoptinmetrics =1687  
			inner join allocclass a1 WITH (NOLOCK) 
				on a1.AcctId = c.AcctId and a1.AllocClassId = c.toAllocClassId
			inner join CodeValue cd WITH (NOLOCK) on c.DistributionRule = cd.CdId
			where c.acctid = @mAcctId  and c.BrkprdId =@mBpid 
			UNION
			select a.acctid,c.TranId,a1.allocclassid as frallocid,a.allocclassid as toallocid,
			a1.allocclassnm as fromallocclassnm,a.allocclassnm as ToAllocclassnm,
			case when c.TrnchOption = 0  then 'Create Special Investment Account'
				 when c.TrnchOption = 1  then 'Follow-Up Investment - Exclude New Partners'
				 when c.TrnchOption = 2  then 'Follow-Up Investment - Include New Partners' 
			else 'Create Special Investment Account' end AS spclinvtmentopt,
			Case Cd.CdId When 494 Then 'By Source Capital %' When 495 Then 'By Target Capital %' 
			When 734 Then 'BY Other Source EP %' When 741 Then 'BY Other Source EP (Partner Level) %' 
			When 743 Then 'By Source EP (Partner Level) %' 
			When 744 Then 'By Target Capacity EP %' 
			when 1150 Then 'By Profile EP %'
			when 1287 Then 'By Source Capacity EP %'
			End  as DistributionRule  ,c.GroupOrder ,case when IsOpening = 1 then 'Opening Carveout' Else 'Closing Carveout' End As Carveouttyp 
			,case when c.IsShutDownSP =1 then 'Yes' Else 'No' End As Closesource
			,case when c.NoSource =1 then 'Yes' Else 'No' End As Nosource
			,case when c.IsIncludePseudo =1 then 'Yes' Else 'No' End As pseudoprtnr
			,case when c.PrtnrSelectionType  =2 then 'Yes' Else 'No' End As spelligible
			,Convert (varchar(10),CreationDt,101) as InvestmentDt
			,c.PriorEPBrkprd
			,c.IsOpening
			from CarveOutTran c WITH (NOLOCK)
			inner join allocclass a WITH (NOLOCK) 
				on a.AcctId = c.AcctId and a.AllocClassId = c.ToAllocClassId and a.IsSidePckt =1
			inner join sidepcktallocclass_stg s WITH (NOLOCK)
				on a.AcctId = s.AcctId and a.AllocClassId =s.allocclassid and s.userid = @muserid and s.isoptinmetrics =1687  
			inner join allocclass a1 WITH (NOLOCK) 
				on a1.AcctId = c.AcctId and a1.AllocClassId = c.FromAllocClassId
			inner join CodeValue cd WITH (NOLOCK) on c.DistributionRule = cd.CdId
			where c.acctid = @mAcctId  and c.BrkprdId =@mBpid 
			) X
			order by x.TranId
			
			------------------------------------------------------------------------------
			------------------GETTING DATA FROM TRANCHBRKPRDCARVEOUT----------------------
			
			create table #TBCO
			(
				acctid int null,brkprdid int null,prtnrid int null,trnchid int null,tranid int null,EPCarveoutAmt numeric (26,2),ep numeric(28,17)
			)
			insert into #TBCO
			select tbco.AcctId,tbco.brkprdid,tbco.PrtnrId,tbco.WithdrawingTrnchId as trnchid,tbco.TranId,tbco.EPCarveoutAmt,tbco.ep
			FROM TranchBrkPrdCarveOut tbco WITH (NOLOCK)
			inner join (select distinct AcctId, tranid from #MAINCODATA where userid = @muserid And AcctId = @mAcctId ) m
				on tbco.AcctId = m.AcctId and tbco.AcctId = @mAcctId and tbco.BrkPrdId = @mBpid and tbco.TranId = m.TranId 

			------------------------------------------------------------------------------ 
			
			Select ExtrnlPrtnrid,i.invstrnm,T.TrancheName ,
			c.AcctId,c.BrkprdId,c.TranId,c.PrtnrId ,c.TrnchId ,
			c.RemainingCapacity ,c.InitAllocation,
			case when cm.tranid is not null then 'Re-allocation ' + CONVERT(varchar(50), cm.StepId) else NULL End as reallocation ,
			convert (varchar(30), cm.CODistrib) as reallocationamt,cm.StepId ,c.WithinNoticePrd ,T.noticeperiod,
			DATEADD(dd,(-1)*T.NoticePeriod,T.FutureRedmptionDt) as NoticePrdStrtDt   
			into #CAPACITY
			From CarveoutSPCapacity c WITH (NOLOCK) 
			Inner Join Partner p WITH (NOLOCK) on p.Acctid =c.Acctid And p.Prtnrid = c.Prtnrid 
			inner join (select distinct AcctId, tranid from #MAINCODATA where userid = @muserid And AcctId = @mAcctId and brkprdid=PriorEPBrkprd) m
				on c.AcctId = m.AcctId and c.TranId = m.TranId
			INNER join tranche T WITH (NOLOCK)
				on c.AcctId = T.AcctId and c.PrtnrId = T.PrtnrId and c.TrnchId = T.TrnchId 
			inner join Investor  i WITH (NOLOCK) on p.InvstrId = i.invstrid
			left join CapacityMaxOutDtl cm WITH (NOLOCK)
				on c.AcctId = cm.AcctId and c.BrkprdId = cm.BrkprdId and c.TranId= cm.TranId and c.PrtnrId = cm.PrtnrId and c.TrnchId = cm.TrnchId 
			
			Where c.AcctId = @mAcctId  And c.BrkprdId = @mBpid  and c.TranId<>0

			---------------------GETTING DATA FOR PRIORBPEP---------------------
			insert into #CAPACITY
			Select ExtrnlPrtnrid,i.invstrnm,T.TrancheName ,
			c.AcctId,@mBpid as BrkprdId,m.TranId,c.PrtnrId ,c.TrnchId ,
			c.RemainingCapacity ,c.InitAllocation,
			case when cm.tranid is not null then 'Re-allocation ' + CONVERT(varchar(50), cm.StepId) else NULL End as reallocation ,
			convert (varchar(30), cm.CODistrib) as reallocationamt,cm.StepId  ,c.WithinNoticePrd ,T.noticeperiod, 
			DATEADD(dd,(-1)*T.NoticePeriod,T.FutureRedmptionDt) as NoticePrdStrtDt 
			From CarveoutSPCapacity c WITH (NOLOCK) 
			Inner Join Partner p WITH (NOLOCK) on p.Acctid =c.Acctid And p.Prtnrid = c.Prtnrid 
			inner join (select distinct AcctId,TranId, IsOpening ,GroupOrder,brkprdid from #MAINCODATA where userid = @muserid And AcctId = @mAcctId and brkprdid<>PriorEPBrkprd) m
				on c.AcctId = m.AcctId and c.IsOpening = m.IsOpening and  c.GroupOrder=1--and c.GroupOrder=m.GroupOrder
			INNER join tranche T WITH (NOLOCK)
				on c.AcctId = T.AcctId and c.PrtnrId = T.PrtnrId and c.TrnchId = T.TrnchId 
			inner join Investor  i WITH (NOLOCK) on p.InvstrId = i.invstrid
			left join CapacityMaxOutDtl cm WITH (NOLOCK)
				on c.AcctId = cm.AcctId and m.TranId = cm.TranId and m.brkprdid=cm.BrkprdId   and c.IsOpening= cm.IsOpening and c.GroupOrder=cm.GroupOrder and c.PrtnrId = cm.PrtnrId and c.TrnchId = cm.TrnchId 
			
			Where c.AcctId = @mAcctId  And c.BrkprdId =dbo.fn_PrevBP(@mAcctId,@mBpid)  and c.TranId=0
			
			-------------------------------------------------------------------
			----------------DATA CORRECTION FOR ASSIGNMENT & TRANSFER--------------------
			SELECT distinct F.AcctId,F.brkprdid,F.capactnid,F.assignerid,F.Assigneeid
			into #100prcntassignment
			FROM ASSIGNMENT F INNER JOIN
			(Select capactnid, assignerid, Sum(maxprct) sumPrct 
			from  
			(Select capactnid,assignerid,Assigneeid, max(PrcntgAmt) maxprct 
				from assignment 
			 Where acctid = @mAcctId  And brkprdid = @mBpid  group by  capactnid,assignerid,Assigneeid
			 ) b
			group by b.capactnid, b.assignerid) asmt 
			
			on f.assignerid = asmt.assignerid and f.CapActnId = asmt.CapActnId
			WHERE sumPrct = 100

		
			;
			with t As
			(
			select *, 
			ROW_NUMBER() OVER(PARTITION BY acctid,capactnid,brkprdid order by acctid,capactnid,brkprdid) AS Row 
			from #100prcntassignment  
			
			)
			
			delete  from t where row>1

			
			select A.AcctId,A.BrkPrdId , A.AssignerID,A.AssignerTrnchId ,A.AssigneeId ,A.AssigneeTrnchId 
			into #finalassignmentdata
			from Assignment A
			inner join #100prcntassignment F
				on A.AcctId = F.AcctId And A.BrkPrdId  = F.BrkPrdId  And A.CapActnId = F.CapActnId And A.AssignerID = F.AssignerID and A.AssigneeId = F.AssigneeId 
			
			if exists(select * from #finalassignmentdata)
				begin
					update C
					Set		C.PrtnrId = C1.AssigneeId 
						,	C.TrnchId = C1.AssigneeTrnchId 
						,	C.ExtrnlPrtnrId= p.ExtrnlPrtnrId 
						,	C.InvstrNm= i.InvstrNm 
						,	c.TrancheName = t.TrancheName 
					from #CAPACITY C
					inner join #finalassignmentdata C1
					on C.AcctId = C1.AcctId And C.BrkprdId = C1.BrkPrdId And C.prtnrid = C1.AssignerID and C.TrnchId  = C1.AssignerTrnchId 
					inner join tranche t on c1.AcctId = t.AcctId and c1.AssigneeId = t.PrtnrId  and c1.AssigneeTrnchId = t.trnchid
					inner join partner p on c1.AcctId = p.AcctId and c1.AssigneeId = p.PrtnrId 
					inner join Investor i on p.InvstrId = i.InvstrId 
					where C.AcctId = @mAcctId 
				end
			drop table #100prcntassignment,#finalassignmentdata
			
			-----------------------------------------------------------------------------


			
			insert into #COTRANFINAL(
			[Partner Id],[Partner Name],[Tranche Name],[Source Allocation Class],[Target Allocation Class],
			[Special Investment Option],[Transfer Distribution Rule],[Group Order],[Carveout Type],[Close Source],[No Source],
			[Pseudo Partner],[Side Pocket Eligible],[Investment Date],[Notice Period],[Notice Period start Date],										
			[Source Base Capital For Carveout],[Remaining Capacity Available],[Initial Allocation],
			[Pre incentive carveout amount],[Percentage Of Transfer],[Target Allocation Class Book Capital],[Reallocation],[Reallocationamt]
			,[frallocid],[toallocid]	)
				
			
				Select distinct Y.*  from
			(
			select C.ExtrnlPrtnrid,C.InvstrNm ,C.TrancheName ,C1.fromallocclassnm ,C1.ToAllocclassnm 
			,C1.spclinvtmentopt,C1.DistributionRule ,c1.GroupOrder ,c1.Carveouttyp ,c1.Closesource ,c1.Nosource ,c1.pseudoprtnr ,c1.spelligible,C1.InvestmentDt,C.noticeperiod,CONVERT(varchar(10), C.NoticePrdStrtDt,101) as NoticePrdStrtDt
			,c2.EPCarveoutAmt ,c.RemainingCapacity,c.InitAllocation  ,0 as [Pre incentive carveout amount] , convert(NUMERIC(28,17), c2.ep *100) as ep,0 as [Target Allocation Class Book Capital],c.reallocation ,c.reallocationamt
			, c1.frallocid,c1.toallocid
			from #CAPACITY C
			INNER JOIN #MAINCODATA C1 on C.AcctId = C1.AcctId And C.TranId = C1.TranId 
			INNER JOIN #TBCO C2 on C.AcctId = C2.AcctId And C.BrkprdId  = C2.brkprdid and C.TranId = c2.tranid and c.PrtnrId =c2.prtnrid and c.TrnchId = c2.trnchid
		    Where C.WithinNoticePrd=0
			Union 

			select C.ExtrnlPrtnrid,C.InvstrNm ,C.TrancheName ,C1.fromallocclassnm ,C1.ToAllocclassnm 
			,C1.spclinvtmentopt,C1.DistributionRule ,c1.GroupOrder ,c1.Carveouttyp ,c1.Closesource ,c1.Nosource ,c1.pseudoprtnr ,c1.spelligible,C1.InvestmentDt,C.noticeperiod,CONVERT(varchar(10), C.NoticePrdStrtDt,101) as NoticePrdStrtDt
			,0 as EPCarveoutAmt ,c.RemainingCapacity,c.InitAllocation  ,0  as [Pre incentive carveout amount] , 0 as ep,0 as [Target Allocation Class Book Capital],c.reallocation ,c.reallocationamt
			, c1.frallocid,c1.toallocid
			from #CAPACITY C
			INNER JOIN #MAINCODATA C1 on C.AcctId = C1.AcctId And C.TranId = C1.TranId
			Where C.WithinNoticePrd=1
			) Y

			select distinct frallocid,toallocid, [Source Allocation Class] as fromallocclassnm,[Target Allocation Class] as ToAllocclassnm,reallocation 
			,dbo.fn_SortString([reallocation]) as reallocationsort 
			into #realloccolumns
			from #COTRANFINAL
			where [reallocation] is not null
		
		
			select distinct  [reallocation],dbo.fn_SortString([reallocation]) as reallocationsort ,StepId as [Order]
				into #Capacitynmcls 
				from #CAPACITY 
				where [reallocation] is not null
				order by [Order]

				Select @cols =   COALESCE(@cols+',','')+ '[' + convert(varchar(50),[reallocation]) + ']'
				from #Capacitynmcls 
				order by [Order],reallocationsort
				
				Select @colsnew = ISNULL(@colsnew + ',' ,'') + 'ISNULL(' + QUOTENAME([reallocation]) + ', ''-'') As ' + QUOTENAME([reallocation])
				from   #Capacitynmcls order by [Order],reallocationsort
				
			select distinct a.AllocClassId, a.AllocClassNm,@muserid as userid 
			into #clsnm 
			from AllocClass a inner join sidepcktallocclass_stg s 
			on a.AcctId = s.acctId and a.AllocClassId = s.allocclassid and s.userid = @muserid and s.isoptinmetrics =1687 
			
			
		if ISNULL(@cols,'') <> ''
			BeGiN
				
			IF OBJECT_ID('tempdb..##Pivot_COTRAN'+Rtrim(Convert(Varchar(10), Coalesce(dbo.fn_GetSessionId(@mUserid),0)))) IS NOT NULL 
					Begin 
					   set @query='Drop Table  ##Pivot_COTRAN'+Rtrim(Convert(Varchar(10), Coalesce(dbo.fn_GetSessionId(@mUserid),0)))
						exec(@query) 
					End
					
				select @query='Select * ' +
				' Into ##Pivot_COTRAN'+Rtrim(Convert(Varchar(10), Coalesce(dbo.fn_GetSessionId(@mUserID),0)))+' 
				from (Select [Partner Id],[Partner Name],[Tranche Name],[Source Allocation Class],[Target Allocation Class],
				[Special Investment Option],[Transfer Distribution Rule],[Group Order],[Carveout Type],[Close Source],[No Source],
				[Pseudo Partner],[Side Pocket Eligible],[Investment Date],[Notice Period],[Notice Period start Date],					
				[Source Base Capital For Carveout],[Remaining Capacity Available],[Initial Allocation],
				[Pre incentive carveout amount],[Percentage Of Transfer],[Target Allocation Class Book Capital] , 
				dbo.fn_sortstring([Partner Id]) as sort,[reallocation],reallocationamt

				from #COTRANFINAL) as t 				
				Pivot (max(reallocationamt) for [reallocation] in ('+@cols+')) as PR '
				exec(@query)
				
				
				select @mclsid = MIN (allocclassid) from #clsnm  where userid = @muserid
				
				while  @mclsid is not null
					begin
						select @mclsnm = allocclassnm from #clsnm where userid = @muserid and AllocClassId  = @mclsid 
						
						if exists(select * from #realloccolumns where frallocid = @mclsid)
							begin
								select [reallocation],reallocationsort into #q from #realloccolumns where frallocid = @mclsid
										
								select @realloc= ISNULL(@realloc + ',' ,'') + 'ISNULL(' + QUOTENAME([reallocation]) + ', ''-'') As ' + QUOTENAME([reallocation])
								from #q order by reallocationsort

								Select @colstot = ISNULL(@colstot + '+ ' ,'') + 'convert(numeric(26,2),ISNULL(' + QUOTENAME([reallocation]) + ', 0 )) ' 
								from   #q order by reallocationsort
								drop table #q
							end
						else
							begin
								if exists(select * from #realloccolumns where toallocid = @mclsid)
									begin	 
										
										select [reallocation],reallocationsort into #p from #realloccolumns where toallocid = @mclsid
										
										select @realloc=  ISNULL(@realloc + ',' ,'') + 'ISNULL(' + QUOTENAME([reallocation]) + ', ''0.00'') As ' + QUOTENAME([reallocation])  
										from #p order by reallocationsort
										
										Select @colstot = ISNULL(@colstot + '+ ' ,'') + 'convert(numeric(26,2),ISNULL(' + QUOTENAME([reallocation]) + ', 0 )) ' 
										from   #p order by reallocationsort
										drop table #p
										
									end
							end

						select @query= 'select  [Partner Id],[Partner Name],[Tranche Name],[Source Allocation Class],[Target Allocation Class],
												[Special Investment Option],[Transfer Distribution Rule],[Group Order],[Carveout Type],[Close Source],
												[No Source],[Pseudo Partner],[Side Pocket Eligible],[Investment Date],[Notice Period],[Notice Period start Date],					
												coalesce([Source Base Capital For Carveout],0) as [Source Base Capital For Carveout],
												coalesce([Remaining Capacity Available],0)  as [Remaining Capacity Available],
												'
												+ case when isnull(@realloc,'')= '' then 'coalesce([Initial Allocation],0) as [Initial Allocation]' 
												else 'coalesce([Initial Allocation],0) as [Initial Allocation],'  + @realloc   end + '
												,coalesce([Pre incentive carveout amount],0) as [Pre incentive carveout amount],
												[Percentage Of Transfer]
												from
												(
						
						
										select  [Partner Id],[Partner Name],[Tranche Name],[Source Allocation Class],[Target Allocation Class],
												[Special Investment Option],[Transfer Distribution Rule],[Group Order],[Carveout Type],[Close Source],
												[No Source],[Pseudo Partner],[Side Pocket Eligible],[Investment Date],[Notice Period],[Notice Period start Date],					
												[Source Base Capital For Carveout],[Remaining Capacity Available],[Initial Allocation]'
												
												+ case when isnull(@realloc,'')= '' then ' ,convert(numeric(26,2),coalesce([Initial Allocation],0)) as [Pre incentive carveout amount],' 
												else ','  + @realloc + ',convert(numeric(26,2),coalesce([Initial Allocation],0)) + ' + @colstot + ' as [Pre incentive carveout amount],'   end + '
												
												[Percentage Of Transfer],[Target Allocation Class Book Capital],[sort] 
												from ##Pivot_COTRAN'+Rtrim(Convert(Varchar(10), Coalesce(dbo.fn_GetSessionId(@mUserID),0))) + ' 
												
										Where [Source Allocation Class] = '''+ @mclsnm + '''
										union
										select  [Partner Id],[Partner Name],[Tranche Name],[Source Allocation Class],[Target Allocation Class],
												[Special Investment Option],[Transfer Distribution Rule],[Group Order],[Carveout Type],[Close Source],
												[No Source],[Pseudo Partner],[Side Pocket Eligible],[Investment Date],[Notice Period],[Notice Period start Date],					
												[Source Base Capital For Carveout],[Remaining Capacity Available],[Initial Allocation]'
												+ case when isnull(@realloc,'')= '' then ' ,convert(numeric(26,2),coalesce([Initial Allocation],0)) as [Pre incentive carveout amount],' 
												else ','  + @realloc + ',convert(numeric(26,2),coalesce([Initial Allocation],0)) + ' + @colstot + ' as [Pre incentive carveout amount],'   end + '
												[Percentage Of Transfer],[Target Allocation Class Book Capital],[sort] 
												from ##Pivot_COTRAN'+Rtrim(Convert(Varchar(10), Coalesce(dbo.fn_GetSessionId(@mUserID),0))) + ' 
												
										Where [Target Allocation Class] = '''+ @mclsnm + '''
										) X order by X.[sort]
										'	
					
						exec(@query)
						select @mclsid = MIN (allocclassid) from #clsnm  where userid = @muserid and allocclassid>@mclsid
						select @realloc=NULL
						select @colstot = NULL
					end
				
				IF OBJECT_ID('tempdb..##Pivot_COTRAN'+Rtrim(Convert(Varchar(10), Coalesce(dbo.fn_GetSessionId(@mUserid),0)))) IS NOT NULL 
					Begin 
					   set @query='Drop Table  ##Pivot_COTRAN'+Rtrim(Convert(Varchar(10), Coalesce(dbo.fn_GetSessionId(@mUserid),0)))
						exec(@query) 
					End
			EnD
		Else
			BeGiN
				select @mclsid = MIN (allocclassid) from #clsnm  where userid = @muserid
				while  @mclsid is not null
					begin
					
						select @mclsnm = allocclassnm from #clsnm where userid = @muserid and AllocClassId  = @mclsid

						select @query= 'Insert into #COTRANFINAL1 
						Select [Partner Id],[Partner Name],[Tranche Name],[Source Allocation Class],[Target Allocation Class],
						[Special Investment Option],[Transfer Distribution Rule],[Group Order],[Carveout Type],[Close Source],[No Source],
						[Pseudo Partner],[Side Pocket Eligible],[Investment Date],[Notice Period],[Notice Period start Date],					
						[Source Base Capital For Carveout],[Remaining Capacity Available],[Initial Allocation],
						[Pre incentive carveout amount],[Percentage Of Transfer],[Target Allocation Class Book Capital]
						,dbo.fn_sortstring([Partner Id]) as sort
						
						from #COTRANFINAL
						Where [Target Allocation Class] = '''+ @mclsnm + ''''
						exec(@query)

						select [Partner Id],[Partner Name],[Tranche Name],[Source Allocation Class],[Target Allocation Class],
						[Special Investment Option],[Transfer Distribution Rule],[Group Order],[Carveout Type],[Close Source],[No Source],
						[Pseudo Partner],[Side Pocket Eligible],[Investment Date],[Notice Period],[Notice Period start Date],					
						[Source Base Capital For Carveout],[Remaining Capacity Available],[Initial Allocation],
						coalesce([Initial Allocation],0) as [Pre incentive carveout amount],[Percentage Of Transfer],[Target Allocation Class Book Capital] 
						from #COTRANFINAL1 order by [SORT]

						select @mclsid = MIN (allocclassid) from #clsnm  where userid = @muserid and allocclassid>@mclsid 
					end
			EnD
			drop table #COTRANFINAL ,#MAINCODATA,#TBCO,#CAPACITY,#Capacitynmcls,#clsnm,#COTRANFINAL1
			
			delete from sidepcktallocclass_stg where isoptinmetrics = 1687 and acctId = @mAcctId 
		end
	
End

GO
SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO

