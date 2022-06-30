/****** Object:  StoredProcedure [dbo].[S_GetTrnchData]    Script Date: 05/27/2016 12:08:42 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[S_GetTrnchData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[S_GetTrnchData]
GO

/****** Object:  StoredProcedure [dbo].[S_GetTrnchData]    Script Date: 05/27/2016 12:08:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[S_GetTrnchData]

@lAcctId Int,
@lPrtnrId Int,
@lBrkprdId Int,
@lWthdrwAmt Int,
@RmTrnchId Int,
@chk Int,
@AsgInd Int,
@AsgnType Int,
@isrollup Int,
@PrevBP Int,
@IsReactivatePseudo Int,
@IsApplyParentChild Int,
@IsAssignmentfromPseudo Int,
@AllowPseudoinAssignment Int

AS
BEGIN
	SET NOCOUNT ON;
	
	Declare @ssql varchar(max)
	
	If @AsgInd = 0
		BEGIN
			If @chk = 0
				BEGIN
					set @ssql = 'Select 0 As [Select]'
				END
			Else
				BEGIN
					set @ssql = 'Select 1 As [Select]'
				END
	        
			If @IsApplyParentChild = 1 --- FITEK07403
				BEGIN
					set @ssql = @ssql + ' , case when PT.acctid is not null Then Coalesce(T.TrancheName,'''') +'' [Pseudo ''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' Else      Coalesce(T.TrancheName,'''') +'' [''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' End  [Allocation Class],Convert(Varchar,B.EndDt,101) EndDt,'
				END
			Else
				BEGIN
					set @ssql = @ssql + ' , case when PT.acctid is not null Then Coalesce(T.TrancheName,'''') +'' [Pseudo ''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' Else      Coalesce(T.TrancheName,'''') +'' [''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' End  [Allocation Class],Convert(Varchar,B.EndDt,101) EndDt,'
					--set @ssql = @ssql + ' , Coalesce(T.TrancheName,COALESCE(a.AllocClassNm, 'General Allocation Class')) [Allocation Class],Convert(Varchar,B.EndDt,101) EndDt,'
				END
	        
			set @ssql = @ssql + ' Coalesce(Case TB.OpBookCap When 0 Then Null Else TB.OpBookCap End,Case TBS.BookCap When 0 Then Null Else TBS.BookCap End,T.BookCap) As BookCap,'
			set @ssql = @ssql + ' Coalesce(Case TB.OpTaxCap When 0 Then Null Else TB.OpTaxCap End,Case TBS.TaxCap When 0 Then Null Else TBS.TaxCap End,T.TaxCap) As [CapActnBookAmt],'
			set @ssql = @ssql + ' Coalesce(Case TB.OpTaxCap When 0 Then Null Else TB.OpTaxCap End,Case TBS.TaxCap When 0 Then Null Else TBS.TaxCap End,T.TaxCap) As [CapActnBookAmt1],'

			set @ssql = @ssql + ' T.TrnchId , T.PrtnrId, '
			--set @ssql = @ssql + ' PT.AcctId[AcctId],0 As [PseudoTrnch],0 As [PseudoTrnch1],1 As Button,Coalesce(ta.AllocClassId,0) As AllocClassId,'
			--*************Modified *******
			set @ssql = @ssql + ' PT.AcctId[AcctId],dbo.fn_GetPseudoTrancheStatus(' + convert(varchar,@lAcctId) + ', ' + convert(varchar,@lPrtnrId) + ',T.TrnchId,' + convert(varchar,@lBrkprdId) + ',' + convert(varchar,@PrevBP) + ',1) as [PseudoTranche],0 As [PseudoTranche1],1 As Button,Coalesce(ta.AllocClassId,0) As AllocClassId,'
			--*****************************
			set @ssql = @ssql + ' 0 As AssignerShareUnit,0 As AssigneeShareUnit, 0 As Button1, 0 As MappedTranchId,0 as finallcf'
		   If (((@IsAssignmentfromPseudo = 1) Or (@AllowPseudoinAssignment = 1)) And ((@AsgnType = 245) Or (@AsgnType = 510) Or (@AsgnType = 511)) And (@isrollup = 0))
				BEGIN
					set @ssql = @ssql + ' ,dbo.fn_IsPseudofromAssignment(' + convert(varchar,@lAcctId) + ', ' + convert(varchar,@lPrtnrId) + ',T.TrnchId,' + convert(varchar,@lBrkprdId) + ',' + convert(varchar,@AsgnType) + ') IsAlreadyPseudo'
				END
		   Else
				BEGIN
					set @ssql = @ssql + ' ,0 IsAlreadyPseudo'
				END
			set @ssql = @ssql + ' , case when PT.acctid is not null Then ''Pseudo '' + COALESCE(a.AllocClassNm, ''General Allocation Class'') Else COALESCE(a.AllocClassNm, ''General Allocation Class'') End aa'

			If @chk = 0
				BEGIN
					set @ssql = @ssql + ' ,0 As [OldSelect]'
				END
			Else
				BEGIN
					set @ssql = @ssql + ' ,1 As [OldSelect]'
				END
			set @ssql = @ssql + ' ,dbo.fn_GetPseudoTrancheStatus(' + convert(varchar,@lAcctId) + ', ' + convert(varchar,@lPrtnrId) + ',T.TrnchId,' + convert(varchar,@lBrkprdId) + ',' + convert(varchar,@PrevBP) + ',1) as [OldPseudoTrnch]'
			set @ssql = @ssql + ' ,0 as IsDirty'
			set @ssql = @ssql + ' ,NULL as [Override Amount]'
	        
			set @ssql = @ssql + ' From Tranche T WITH (NOLOCK) '
			set @ssql = @ssql + ' Inner Join BrkPrd B WITH (NOLOCK) On T.AcctId=B.AcctId And T.StartBp=B.BrkprdId'
	        
			set @ssql = @ssql + ' Left Join TranchAllocClass ta WITH (NOLOCK) '
			set @ssql = @ssql + ' Inner Join AllocClass A WITH (NOLOCK) on ta.AcctId = a.Acctid and ta.AllocClassId = a.AllocClassId And ((a.IsGenAllocClass = 1 and IsHotGeneral = 0 ) Or IsSidePckt = 1)'
			set @ssql = @ssql + ' On ta.AcctId =t.Acctid and ta.PrtnrId = t.Prtnrid and T.TrnchId = ta.TrnchId'
	        
			set @ssql = @ssql + ' Left Join Pseudotranche PT WITH (NOLOCK) '  --- Changed on 07.06.2004
			set @ssql = @ssql + ' On PT.AcctId =t.Acctid and PT.PrtnrId = t.Prtnrid and T.TrnchId = PT.TrnchId'  --- Changed on 07.06.2004
			set @ssql = @ssql + ' And (PT.StartBp<=' + convert(varchar,@lBrkprdId) + ' And (PT.EndBP Is Null Or PT.EndBP>=' + convert(varchar,@lBrkprdId) + ')) '  --- Changed on 07.06.2004
	        
			set @ssql = @ssql + ' Left Join TranchBrkPrd TB WITH (NOLOCK) On T.AcctId = TB.AcctId And T.PrtnrId = TB.PrtnrId And T.TrnchId = TB.TrnchId'
			set @ssql = @ssql + ' And TB.BrkPrdId = ' + convert(varchar,@lBrkprdId)
			set @ssql = @ssql + ' Left Join TranchBrkPrd TBS On T.AcctId = TBS.AcctId And T.PrtnrId = TBS.PrtnrId And T.TrnchId = TBS.TrnchId'
			set @ssql = @ssql + ' And TBS.BrkPrdId = ' + convert(varchar,@PrevBP)
	        
	        
	        
			set @ssql = @ssql + ' Left Join'
			set @ssql = @ssql + '     (Select T.acctid, t.prtnrid, t.trnchid'
			set @ssql = @ssql + ' From Tranche T'
			set @ssql = @ssql + ' Inner Join TranchCapitalActn tca On tca.AcctId = T.AcctId And tca.PrtnrId = T.PrtnrId And tca.TrnchId = T.TrnchId'
			set @ssql = @ssql + ' Inner Join CapitalActn ca On ca.AcctId = tca.AcctId And ca.CapActnId = tca.CapActnId And ca.CapActnTyp = 363'
	        
			If @AsgnType = 245
				BEGIN
					set @ssql = @ssql + ' Inner Join CarveoutTran cout on ca.AcctId = cout.AcctId And ca.CapActnId = cout.CapActnId'
					set @ssql = @ssql + ' And cout.BrkPrdId = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And cout.IsOpening = 0'
				END
	        
			set @ssql = @ssql + ' Where T.StartBp = ' + convert(varchar,@lBrkprdId)
			set @ssql = @ssql + ' And T.AcctId = ' + convert(varchar,@lAcctId)
			set @ssql = @ssql + ' And T.PrtnrId = ' + convert(varchar,@lPrtnrId)
			set @ssql = @ssql + ' ) crv'
			set @ssql = @ssql + ' On t.AcctId = crv.AcctId And t.PrtnrId = crv.PrtnrId And t.TrnchId = crv.TrnchId'
			set @ssql = @ssql + ' Where crv.acctid is NUll '
	        
			set @ssql = @ssql + ' And T.AcctId=' + convert(varchar,@lAcctId) + ' And T.PrtnrId=' + convert(varchar,@lPrtnrId) + ' And T.TrnchId<> ' + convert(varchar,@RmTrnchId)
			--set @ssql = @ssql + ' And PT.AcctId is NULL '  'Changed on 07.06.2004
			--***************Modified By Sudip Roy April 28,2010
			If ((@IsReactivatePseudo <> 1) And (@AllowPseudoinAssignment = 0)) --*********as discussed with Santanu Da
				BEGIN
					set @ssql = @ssql + ' And PT.AcctId is NULL '
				END
	            
	--'        If IsReactivatePseudo <> 1 And AsgnType <> 511 Then
	--'            set @ssql = @ssql + ' And PT.AcctId is NULL '  'Changed on 07.06.2004
	--'        End If
	        
	--'''        If AsgnType = 245 Then
				set @ssql = @ssql + ' And ((T.StartBp<' + convert(varchar,@lBrkprdId) + ' And (T.EndBP Is Null Or T.EndBP>=' + convert(varchar,@lBrkprdId) + ')) Or '
				set @ssql = @ssql + ' (T.StartBp = ' + convert(varchar,@lBrkprdId)
				set @ssql = @ssql + ' And (T.EndBp Is Null Or T.EndBp >= ' + convert(varchar,@lBrkprdId) + ') And T.IsOpeningContrb=1))'
	--'''        Else
	--'''            set @ssql = @ssql + ' And ((T.StartBp<' + lBrkprdId + ' And (T.EndBP Is Null Or T.EndBP>=' + PrevBP + ')) Or '
	--'''            set @ssql = @ssql + ' (T.StartBp = ' + lBrkprdId
	--'''            set @ssql = @ssql + ' And (T.EndBp Is Null Or T.EndBp >= ' + PrevBP + ') And T.IsOpeningContrb=1))'
	--'''        End If
	        
	--        ''If Chk = 0 Then
	--'''            set @ssql = @ssql + ' And T.TrnchId Not In'
	--'''            set @ssql = @ssql + ' (Select AssigneeTrnchId From Assignment'
	--'''            set @ssql = @ssql + ' Where AcctId = ' + lAcctId
	--'''            set @ssql = @ssql + ' And BrkPrdId = ' + lBrkprdId
	--'''            set @ssql = @ssql + ' And AssigneeId = ' + lPrtnrId
	--'''            set @ssql = @ssql + ' And AssignmentType = 510)'
	--        ''End If

			set @ssql = @ssql + ' And T.TrnchId Not In' --- FITEK06606			
			If ((@IsAssignmentfromPseudo = 0) And (@AllowPseudoinAssignment = 0))
				BEGIN
					set @ssql = @ssql + ' (Select Tca.TrnchId From TranchCapitalActn Tca'
					set @ssql = @ssql + ' Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
					set @ssql = @ssql + ' Where Ca.AcctId = ' + convert(varchar,@lAcctId)
					set @ssql = @ssql + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId)
					If @AsgnType <> 245
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649))'
						END
					Else
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39))'
						END
				END
			Else If (((@IsAssignmentfromPseudo = 1) And (@AllowPseudoinAssignment = 1)) And ((@AsgnType = 245) Or (@AsgnType = 510) Or (@AsgnType = 511)) And (@isrollup = 0))
				BEGIN
					set @ssql = @ssql + '( select t.trnchid from ('
					set @ssql = @ssql + ' (Select Tca.AcctId,Tca.PrtnrId,Tca.TrnchId From TranchCapitalActn Tca'
					set @ssql = @ssql + ' Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
					set @ssql = @ssql + ' Where Ca.AcctId = ' + convert(varchar,@lAcctId)
					set @ssql = @ssql + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId)
					If @AsgnType <> 245
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649)'
						END
					Else
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39)'
						END
					set @ssql = @ssql + ' ) t '
					set @ssql = @ssql + ' left join'
						set @ssql = @ssql + '('
							set @ssql = @ssql + ' select distinct a.acctid,a.prtnrid,a.trnchid,a.pseudo from'
							set @ssql = @ssql + '( '
							set @ssql = @ssql + '    Select Tca.AcctId,Tca.PrtnrId,Tca.TrnchId,case when p.AcctId Is null then 0 else 1 end as pseudo  From TranchCapitalActn Tca'
							set @ssql = @ssql + '    Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
							set @ssql = @ssql + '    left join PseudoTranche p  On Tca.AcctId =p.Acctid and Tca.PrtnrId = p.Prtnrid and Tca.TrnchId = p.TrnchId'
							If @AsgnType = 510
								BEGIN
									set @ssql = @ssql + '    And (p.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (p.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (p.EndBP Is Null Or p.EndBP<' + convert(varchar,@lBrkprdId) + ')'
								END
							Else If @AsgnType = 511
								BEGIN
									set @ssql = @ssql + '        And (p.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (p.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (p.EndBP Is Null Or p.EndBP<' + convert(varchar,@lBrkprdId) + ')'
								END
							Else If @AsgnType = 245
								BEGIN
									set @ssql = @ssql + '        And ((p.StartBp <=' + convert(varchar,@lBrkprdId) + ' or (p.startBp=dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ') )) And (p.EndBP Is Null Or p.EndBP<dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ')))'
								END
							set @ssql = @ssql + '    Where Ca.AcctId = ' + convert(varchar,@lAcctId) + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId) + '  And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId) + ' '
							If @AsgnType <> 245
								BEGIN
									set @ssql = @ssql + ' And Ca.CapActnTyp In (649)'
								END
							Else
								BEGIN
									set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39)'
								END
						
							set @ssql = @ssql + ' ) a'
							set @ssql = @ssql + '    inner join TranchCoRelationHist co'
							set @ssql = @ssql + '        on a.AcctId =co.AcctId and a.PrtnrId =co.PrtnrId and a.TrnchId =co.MappedTrnchId'
							If @AsgnType = 510
								BEGIN
									set @ssql = @ssql + '   And (co.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (co.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (co.EndBP Is Null Or co.EndBP<' + convert(varchar,@lBrkprdId) + ')'
								END
							Else If @AsgnType = 511
								BEGIN
									set @ssql = @ssql + '   And (co.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (co.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (co.EndBP Is Null Or co.EndBP<' + convert(varchar,@lBrkprdId) + ')'
								END
							Else If @AsgnType = 245
								BEGIN
									set @ssql = @ssql + '   And ((co.StartBp <=' + convert(varchar,@lBrkprdId) + ' or (co.startBp=dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ') )) And (co.EndBP Is Null Or co.EndBP<dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ')))'
								END
							set @ssql = @ssql + '    Where a.pseudo = 1'
					
						set @ssql = @ssql + ' ) ps'
						set @ssql = @ssql + ' on T.AcctId=ps.AcctId and T.PrtnrId=ps.PrtnrId And T.TrnchId=ps.TrnchId'
					set @ssql = @ssql + ' )'
					
					set @ssql = @ssql + ' where ps .AcctId is null'
					set @ssql = @ssql + ' )'
				END
			Else
				BEGIN
					set @ssql = @ssql + ' (Select Tca.TrnchId From TranchCapitalActn Tca'
					set @ssql = @ssql + ' Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
					set @ssql = @ssql + ' Where Ca.AcctId = ' + convert(varchar,@lAcctId)
					set @ssql = @ssql + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId)
					If @AsgnType <> 245
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649))'
						END
					Else
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39))'
						END
				END
	--'        set @ssql = @ssql + ' Order By T.TrnchId' '[Allocation Class]'
	--'        set @ssql = @ssql + ' Order By [Allocation Class]'
			set @ssql = @ssql + ' Order By aa'
        END
    Else If @AsgInd = 1
		BEGIN
			set @ssql = ' Select [Select],[Allocation Class],EndDt,BookCap,CapActnBookAmt,CapActnBookAmt1,TrnchId,PrtnrId,AcctId,PseudoTrnch As PseudoTranche,'
			set @ssql = @ssql + ' PseudoTrnch1 , Button, AllocClassId,0 As AssignerShareUnit,0 As AssigneeShareUnit, 0 As Button1, 0 As MappedTranchId,Coalesce(FinalLcf,0)  as  FinalLcf,IsAlreadyPseudo,aa,OldSelect,OldPseudoTrnch,IsDirty,OvrdAmnt as [Override Amount],CapActId  From '
	    
			If @chk = 0
				BEGIN
					set @ssql = @ssql + ' (Select 0 As [Select]'
				END
			Else
				BEGIN
					set @ssql = @ssql + ' (Select 1 As [Select]'
				END
	        
			set @ssql = @ssql + ' ,Case When T.EndBp Is Null Then 0 Else (Case When (Select Sum(Coalesce(Asg.PrcntgAmt,0)) from Assignment Asg Where Asg.AcctId = ' + convert(varchar,@lAcctId) --- FITEK06687
			set @ssql = @ssql + ' And Asg.BrkPrdId = ' + convert(varchar,@lBrkprdId) + ' And Asg.AssignerId = ' + convert(varchar,@lPrtnrId) + ' And Asg.IsBalance = 0 '
			set @ssql = @ssql + ' And Asg.AssignerTrnchId = T.TrnchId) = 100 Then 1 When T.EndBp = ' + convert(varchar,@lBrkprdId) + ' Then 1 Else 2 End) End As x' ---FITEK06431 ''' FITEK06547
	        
			If @IsApplyParentChild = 1 --- FITEK07403
				BEGIN
					set @ssql = @ssql + ' , case when PT.acctid is not null Then Coalesce(T.TrancheName,'''') +'' [Pseudo ''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' Else      Coalesce(T.TrancheName,'''') +'' [''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' End  [Allocation Class],Convert(Varchar,B.EndDt,101) EndDt,'
				END
			Else
				BEGIN
					set @ssql = @ssql + ' , case when PT.acctid is not null Then Coalesce(T.TrancheName,'''') +'' [Pseudo ''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' Else      Coalesce(T.TrancheName,'''') +'' [''+ COALESCE(a.AllocClassNm, ''General Allocation Class'')+'']'' End  [Allocation Class],Convert(Varchar,B.EndDt,101) EndDt,'
					--set @ssql = @ssql + ' , Coalesce(T.TrancheName,COALESCE(a.AllocClassNm, 'General Allocation Class')) [Allocation Class],Convert(Varchar,B.EndDt,101) EndDt,'
				END

			--set @ssql = @ssql + ' , Coalesce(T.TrancheName,COALESCE(a.AllocClassNm, 'General Allocation Class')) [Allocation Class],Convert(Varchar,B.EndDt,101) EndDt,'
	        
			set @ssql = @ssql + ' Coalesce(Case TB.OpBookCap When 0 Then Null Else TB.OpBookCap End,Case TBS.BookCap When 0 Then Null Else TBS.BookCap End,T.BookCap) As BookCap,'
			set @ssql = @ssql + ' Coalesce(Case TB.OpTaxCap When 0 Then Null Else TB.OpTaxCap End,Case TBS.TaxCap When 0 Then Null Else TBS.TaxCap End,T.TaxCap) As [CapActnBookAmt],'
			set @ssql = @ssql + ' Coalesce(Case TB.OpTaxCap When 0 Then Null Else TB.OpTaxCap End,Case TBS.TaxCap When 0 Then Null Else TBS.TaxCap End,T.TaxCap) As [CapActnBookAmt1],'
	        
			set @ssql = @ssql + ' T.TrnchId,T.PrtnrId,'
			set @ssql = @ssql + ' PT.AcctId[AcctId],'  --- Changed on 07.06.2004
			set @ssql = @ssql + ' dbo.fn_GetPseudoTrancheStatus(' + convert(varchar,@lAcctId) + ', ' + convert(varchar,@lPrtnrId) + ',T.TrnchId,dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + '),' + convert(varchar,@PrevBP) + ',1) [PseudoTrnch],'
			set @ssql = @ssql + ' dbo.fn_GetPseudoTrancheStatus(' + convert(varchar,@lAcctId) + ', ' + convert(varchar,@lPrtnrId) + ',T.TrnchId,dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + '),' + convert(varchar,@PrevBP) + ',1) [PseudoTrnch1],1 As Button,Coalesce(ta.AllocClassId,0) As AllocClassId ,coalesce(TI.lcfoverride,TI.lcfcalc,0) as FinalLcf'
		   If (((@IsAssignmentfromPseudo = 1) Or (@AllowPseudoinAssignment = 1)) And ((@AsgnType = 245) Or (@AsgnType = 510) Or (@AsgnType = 511)) And (@isrollup = 0))
				BEGIN
					set @ssql = @ssql + ' ,dbo.fn_IsPseudofromAssignment(' + convert(varchar,@lAcctId) + ', ' + convert(varchar,@lPrtnrId) + ',T.TrnchId,' + convert(varchar,@lBrkprdId) + ',' + convert(varchar,@AsgnType) + ') IsAlreadyPseudo'
				END
		   Else
				BEGIN
					set @ssql = @ssql + ' ,0 IsAlreadyPseudo'
				END
			set @ssql = @ssql + ' ,case when PT.acctid is not null Then ''Pseudo '' + COALESCE(a.AllocClassNm, ''General Allocation Class'') Else COALESCE(a.AllocClassNm, ''General Allocation Class'') End  aa'
	        
			If @chk = 0
				BEGIN
					set @ssql = @ssql + ' ,0 As [OldSelect]'
				END
			Else
				BEGIN
					set @ssql = @ssql + ' ,1 As [OldSelect]'
				END
			set @ssql = @ssql + ' ,dbo.fn_GetPseudoTrancheStatus(' + convert(varchar,@lAcctId) + ', ' + convert(varchar,@lPrtnrId) + ',T.TrnchId,dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + '),' + convert(varchar,@PrevBP) + ',1) as [OldPseudoTrnch]'
			set @ssql = @ssql + ' ,0 as IsDirty'
			set @ssql = @ssql + ' ,NULL as OvrdAmnt,pairid as CapActId'
	        
			set @ssql = @ssql + ' From Tranche T WITH (NOLOCK) '
			set @ssql = @ssql + ' Inner Join BrkPrd B WITH (NOLOCK) on T.AcctId=B.AcctId And T.StartBp=B.BrkprdId'
			set @ssql = @ssql + '     left Join Pseudotranche P WITH (NOLOCK)'
			set @ssql = @ssql + '         On P.AcctId =t.Acctid and P.PrtnrId = t.Prtnrid'
			set @ssql = @ssql + '             and T.TrnchId = P.TrnchId And (P.StartBp <= ' + convert(varchar,@lBrkprdId) + '  And (P.EndBP Is Null Or P.EndBP>=' + convert(varchar,@lBrkprdId) + ' ))'
	        
			set @ssql = @ssql + ' Left Join TranchAllocClass ta WITH (NOLOCK) '
			set @ssql = @ssql + ' Inner Join AllocClass A WITH (NOLOCK) on ta.AcctId = a.Acctid and ta.AllocClassId = a.AllocClassId And ((a.IsGenAllocClass = 1 and IsHotGeneral = 0 ) Or IsSidePckt = 1)'
			set @ssql = @ssql + ' On ta.AcctId =t.Acctid and ta.PrtnrId = t.Prtnrid and T.TrnchId = ta.TrnchId'
	        
			set @ssql = @ssql + ' Left Join Pseudotranche PT WITH (NOLOCK) '  --- Changed on 07.06.2004
			set @ssql = @ssql + ' On PT.AcctId =t.Acctid and PT.PrtnrId = t.Prtnrid and T.TrnchId = PT.TrnchId ' --- Changed on 07.06.2004'
	        
			If @AsgnType <> 245 --- op
				BEGIN
					set @ssql = @ssql + ' And (PT.StartBp < ' + convert(varchar,@lBrkprdId) + ' And (PT.EndBP Is Null Or PT.EndBP>=' + convert(varchar,@lBrkprdId) + ')) '  --- Changed on 07.06.2004
				END
			Else
				BEGIN
					set @ssql = @ssql + ' And (PT.StartBp <= ' + convert(varchar,@lBrkprdId) + ' And (PT.EndBP Is Null Or PT.EndBP>=' + convert(varchar,@lBrkprdId) + ')) '  --- Changed on 07.06.2004
				END
	        
			set @ssql = @ssql + ' Left Join TranchBrkPrd TB WITH (NOLOCK) On T.AcctId = TB.AcctId And T.PrtnrId = TB.PrtnrId And T.TrnchId = TB.TrnchId'
			set @ssql = @ssql + ' And TB.BrkPrdId = ' + convert(varchar,@lBrkprdId)
			set @ssql = @ssql + ' Left Join TranchBrkPrd TBS On T.AcctId = TBS.AcctId And T.PrtnrId = TBS.PrtnrId And T.TrnchId = TBS.TrnchId'
			set @ssql = @ssql + ' And TBS.BrkPrdId = ' + convert(varchar,@PrevBP)
	        
	        
			--**************Modified By Sudip Roy on Sep 13 2010 ****** CITI11626
			If @AsgnType = 245 --*********** Closing
				BEGIN
					 set @ssql = @ssql + ' Left Join Tempincentive TI On TI.AcctId = TB.AcctId And TI.PrtnrId = TB.PrtnrId And TI.TrnchId = TB.TrnchId'
					 set @ssql = @ssql + ' And TI.BrkPrdId = ' + convert(varchar,@lBrkprdId)
				END
	        
			If @AsgnType = 510 --********** Opening
				BEGIN
					 set @ssql = @ssql + ' Left Join Tempincentive TI On TI.AcctId = TBS.AcctId And TI.PrtnrId = TBS.PrtnrId And TI.TrnchId = TBS.TrnchId'
					 set @ssql = @ssql + ' And TI.BrkPrdId = ' + convert(varchar,@PrevBP)
				END
	        
			If @AsgnType = 511 --*********** Addon
				BEGIN
					 set @ssql = @ssql + ' Left Join Tempincentive TI On TI.AcctId = TB.AcctId And TI.PrtnrId = TB.PrtnrId And TI.TrnchId = TB.TrnchId'
					 set @ssql = @ssql + ' And TI.BrkPrdId = ' + convert(varchar,@lBrkprdId)
				END
			--*********************************************************
	        
			set @ssql = @ssql + ' Left Join'
			set @ssql = @ssql + '     (Select T.acctid, t.prtnrid, t.trnchid'
			set @ssql = @ssql + ' From Tranche T'
			set @ssql = @ssql + ' Inner Join TranchCapitalActn tca On tca.AcctId = T.AcctId And tca.PrtnrId = T.PrtnrId And tca.TrnchId = T.TrnchId'
			set @ssql = @ssql + ' Inner Join CapitalActn ca On ca.AcctId = tca.AcctId And ca.CapActnId = tca.CapActnId And ca.CapActnTyp = 363'
	        
			If @AsgnType = 245
				BEGIN
					set @ssql = @ssql + ' Inner Join CarveoutTran cout on ca.AcctId = cout.AcctId And ca.CapActnId = cout.CapActnId'
					set @ssql = @ssql + ' And cout.BrkPrdId = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And cout.IsOpening = 0'
				END
	        
			set @ssql = @ssql + ' Where T.StartBp = ' + convert(varchar,@lBrkprdId)
			set @ssql = @ssql + ' And T.AcctId = ' + convert(varchar,@lAcctId)
			set @ssql = @ssql + ' And T.PrtnrId = ' + convert(varchar,@lPrtnrId)
			set @ssql = @ssql + ' ) crv'
			set @ssql = @ssql + ' On t.AcctId = crv.AcctId And t.PrtnrId = crv.PrtnrId And t.TrnchId = crv.TrnchId'
			set @ssql = @ssql + ' Where crv.acctid is NUll '
	        
			set @ssql = @ssql + ' And T.AcctId=' + convert(varchar,@lAcctId) + ' And T.PrtnrId=' + convert(varchar,@lPrtnrId) + ' And T.TrnchId<> ' + convert(varchar,@RmTrnchId)
			If (((@IsAssignmentfromPseudo = 0) And (@AllowPseudoinAssignment = 0)) Or (@isrollup = 1))
				BEGIN
					set @ssql = @ssql + ' And PT.AcctId is NULL '  --- Changed on 07.06.2004
				END
			If @AsgnType = 245
				BEGIN
					set @ssql = @ssql + ' And ((T.StartBp<' + convert(varchar,@lBrkprdId) + ' And (T.EndBP Is Null Or T.EndBP>=' + convert(varchar,@lBrkprdId) + ')) Or '
					set @ssql = @ssql + ' (T.StartBp = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And (T.EndBp Is Null Or T.EndBp >= ' + convert(varchar,@lBrkprdId) + ') And T.IsOpeningContrb=1))'
				END
			Else
				BEGIN
					set @ssql = @ssql + ' And ((T.StartBp<' + convert(varchar,@lBrkprdId) + ' And (T.EndBP Is Null Or T.EndBP>=' + convert(varchar,@PrevBP) + ')) Or '
					set @ssql = @ssql + ' (T.StartBp = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And (T.EndBp Is Null Or T.EndBp >= ' + convert(varchar,@PrevBP) + ') And T.IsOpeningContrb=1))'
				END
	    
			If @AsgnType <> 245
				BEGIN
					set @ssql = @ssql + ' And T.TrnchId Not In'
					set @ssql = @ssql + ' (Select AssigneeTrnchId From Assignment'
					set @ssql = @ssql + ' Where AcctId = ' + convert(varchar,@lAcctId)
					set @ssql = @ssql + ' And BrkPrdId = ' + convert(varchar,@lBrkprdId)
					set @ssql = @ssql + ' And AssigneeId = ' + convert(varchar,@lPrtnrId)
					set @ssql = @ssql + ' And AssignmentType In (510))' --- FITEK06684
				END
	        
	--'''        If AsgnType = 510 Or AsgnType = 511 Then
	--'''            set @ssql = @ssql + ' And dbo.fn_CanWithdrawTranch (' + lAcctId + ' , ' + lPrtnrId + ', T.TrnchId , 0 ,Null ) <> 0'
	--'''        Else
	--'''            set @ssql = @ssql + ' And dbo.fn_CanWithdrawTranch (' + lAcctId + ' , ' + lPrtnrId + ', T.TrnchId , 1,Null ) <> 0'
	--'''        End If
	        
			set @ssql = @ssql + ' ) M Where x <> 2'

			set @ssql = @ssql + ' And M.TrnchId Not In' --- FITEK06606
		If ((@IsAssignmentfromPseudo = 0) And (@AllowPseudoinAssignment = 0))
			BEGIN
				set @ssql = @ssql + ' (Select Tca.TrnchId From TranchCapitalActn Tca'
				set @ssql = @ssql + ' Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
				set @ssql = @ssql + ' Where Ca.AcctId = ' + convert(varchar,@lAcctId)
				set @ssql = @ssql + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId)
				set @ssql = @ssql + ' And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId)
				If @AsgnType <> 245
					BEGIN
						set @ssql = @ssql + ' And Ca.CapActnTyp In (649))'
					END
				Else
					BEGIN
						set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39))'
					END
			END
		Else If (((@IsAssignmentfromPseudo = 1) And (@AllowPseudoinAssignment = 1)) And ((@AsgnType = 245) Or (@AsgnType = 510) Or (@AsgnType = 511)) And (@isrollup = 0))
			BEGIN
				set @ssql = @ssql + '( select t.trnchid from ('
				set @ssql = @ssql + ' (Select Tca.AcctId,Tca.PrtnrId,Tca.TrnchId From TranchCapitalActn Tca'
				set @ssql = @ssql + ' Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
				set @ssql = @ssql + ' Where Ca.AcctId = ' + convert(varchar,@lAcctId)
				set @ssql = @ssql + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId)
				set @ssql = @ssql + ' And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId)
				If @AsgnType <> 245
					BEGIN
						set @ssql = @ssql + ' And Ca.CapActnTyp In (649)'
					END
				Else
					BEGIN
						set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39)'
					END
				set @ssql = @ssql + ' ) t '
				set @ssql = @ssql + ' left join'
					set @ssql = @ssql + '('
						set @ssql = @ssql + ' select distinct a.acctid,a.prtnrid,a.trnchid,a.pseudo from'
						set @ssql = @ssql + '( '
						set @ssql = @ssql + '    Select Tca.AcctId,Tca.PrtnrId,Tca.TrnchId,case when p.AcctId Is null then 0 else 1 end as pseudo  From TranchCapitalActn Tca'
						set @ssql = @ssql + '    Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
						set @ssql = @ssql + '    left join PseudoTranche p  On Tca.AcctId =p.Acctid and Tca.PrtnrId = p.Prtnrid and Tca.TrnchId = p.TrnchId'
						If @AsgnType = 510
							BEGIN
								set @ssql = @ssql + '    And (p.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (p.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (p.EndBP Is Null Or p.EndBP<' + convert(varchar,@lBrkprdId) + ')'
							END
						Else If @AsgnType = 511
							BEGIN
								set @ssql = @ssql + '        And (p.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (p.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (p.EndBP Is Null Or p.EndBP<' + convert(varchar,@lBrkprdId) + ')'
							END
						Else If @AsgnType = 245
							BEGIN
								set @ssql = @ssql + '        And ((p.StartBp <=' + convert(varchar,@lBrkprdId) + ' or (p.startBp=dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ') )) And (p.EndBP Is Null Or p.EndBP<dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ')))'
							END
						set @ssql = @ssql + '    Where Ca.AcctId = ' + convert(varchar,@lAcctId) + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId) + '  And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId) + ' '
					If @AsgnType <> 245
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649)'
						END
					Else
						BEGIN
							set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39)'		    
						END
						set @ssql = @ssql + ' ) a'
						set @ssql = @ssql + '    inner join TranchCoRelationHist co'
						set @ssql = @ssql + '        on a.AcctId =co.AcctId and a.PrtnrId =co.PrtnrId and a.TrnchId =co.MappedTrnchId'
						If @AsgnType = 510
							BEGIN
								set @ssql = @ssql + '   And (co.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (co.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (co.EndBP Is Null Or co.EndBP<' + convert(varchar,@lBrkprdId) + ')'
							END
						Else If @AsgnType = 511
							BEGIN
								set @ssql = @ssql + '   And (co.StartBp < ' + convert(varchar,@lBrkprdId) + ' or (co.startBp=' + convert(varchar,@lBrkprdId) + ' )) And (co.EndBP Is Null Or co.EndBP<' + convert(varchar,@lBrkprdId) + ')'
							END
						Else If @AsgnType = 245
							BEGIN
								set @ssql = @ssql + '   And ((co.StartBp <=' + convert(varchar,@lBrkprdId) + ' or (co.startBp=dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ') )) And (co.EndBP Is Null Or co.EndBP<dbo.fn_NextBp(' + convert(varchar,@lAcctId) + ',' + convert(varchar,@lBrkprdId) + ')))'
							END
						set @ssql = @ssql + '    Where a.pseudo = 1'
		        
					set @ssql = @ssql + ' ) ps'
					set @ssql = @ssql + ' on T.AcctId=ps.AcctId and T.PrtnrId=ps.PrtnrId And T.TrnchId=ps.TrnchId'
				set @ssql = @ssql + ' )'
		        
				set @ssql = @ssql + ' where ps .AcctId is null'
				set @ssql = @ssql + ' )'
			END
		Else
			BEGIN
				set @ssql = @ssql + ' (Select Tca.TrnchId From TranchCapitalActn Tca'
				set @ssql = @ssql + ' Inner Join CapitalActn Ca On Tca.AcctId = Ca.AcctId And Tca.CapActnId = Ca.CapActnId'
				set @ssql = @ssql + ' Where Ca.AcctId = ' + convert(varchar,@lAcctId)
				set @ssql = @ssql + ' And Ca.BrkPrdId = ' + convert(varchar,@lBrkprdId)
				set @ssql = @ssql + ' And Tca.PrtnrId = ' + convert(varchar,@lPrtnrId)
				If @AsgnType <> 245
					BEGIN
						set @ssql = @ssql + ' And Ca.CapActnTyp In (649))'
					END
				Else
					BEGIN
						set @ssql = @ssql + ' And Ca.CapActnTyp In (649,276,39))'
					END
			END

	--'        set @ssql = @ssql + ' Order By TrnchId' '[Allocation Class]'
	--'        set @ssql = @ssql + ' Order By [Allocation Class]'
			set @ssql = @ssql + ' Order By aa'
        
		END
    
    exec(@ssql)
	
END


GO

