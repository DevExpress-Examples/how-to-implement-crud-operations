﻿Imports System
Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Validation
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Text

Namespace DevExpress.CRUD.DataModel.EntityFramework
	Public Class EntityFrameworkCRUDDataProvider(Of TContext As DbContext, TEntity As Class, T As Class, TKey)
		Inherits EntityFrameworkDataProvider(Of TContext, TEntity, T)
		Implements ICRUDDataProvider(Of T)

		Private ReadOnly getKey As Func(Of T, TKey)
		Private ReadOnly getEntityKey As Func(Of TEntity, TKey)
		Private ReadOnly setKey As Action(Of T, TKey)
		Private ReadOnly applyProperties As Action(Of T, TEntity)

		Public Sub New(ByVal createContext As Func(Of TContext), ByVal getDbSet As Func(Of TContext, DbSet(Of TEntity)), ByVal getEnityExpression As Expression(Of Func(Of TEntity, T)), ByVal getKey As Func(Of T, TKey), ByVal getEntityKey As Func(Of TEntity, TKey), ByVal setKey As Action(Of T, TKey), ByVal applyProperties As Action(Of T, TEntity))
			MyBase.New(createContext, getDbSet, getEnityExpression)
			Me.getKey = getKey
			Me.getEntityKey = getEntityKey
			Me.setKey = setKey
			Me.applyProperties = applyProperties
		End Sub

		Private Sub ICRUDDataProviderGeneric_Delete(ByVal obj As T) Implements ICRUDDataProvider(Of T).Delete
			Using context = createContext()
				Dim entity = getDbSet(context).Find(getKey(obj))
				If entity Is Nothing Then
					Throw New NotImplementedException("The deleted row does not exist in a database anymore. Handle this case according to your requirements.")
				End If
				getDbSet(context).Remove(entity)
				SaveChanges(context)
			End Using
		End Sub

		Private Sub ICRUDDataProviderGeneric_Create(ByVal obj As T) Implements ICRUDDataProvider(Of T).Create
			Using context = createContext()
				Dim entity = getDbSet(context).Create()
				getDbSet(context).Add(entity)
				applyProperties(obj, entity)
				SaveChanges(context)
				setKey(obj, getEntityKey(entity))
			End Using
		End Sub

		Private Sub ICRUDDataProviderGeneric_Update(ByVal obj As T) Implements ICRUDDataProvider(Of T).Update
			Using context = createContext()
				Dim entity = getDbSet(context).Find(getKey(obj))
				If entity Is Nothing Then
					Throw New NotImplementedException("The modified row does not exist in a database anymore. Handle this case according to your requirements.")
				End If
				applyProperties(obj, entity)
				SaveChanges(context)
			End Using
		End Sub

		Private Shared Sub SaveChanges(ByVal context As TContext)
			Try
				context.SaveChanges()
			Catch e As Exception
				Throw ConvertException(e)
			End Try
		End Sub

		Private Shared Function ConvertException(ByVal e As Exception) As DbException
			Dim entityValidationException = TryCast(e, DbEntityValidationException)
			If entityValidationException IsNot Nothing Then
				Dim stringBuilder As New StringBuilder()
				For Each validationResult In entityValidationException.EntityValidationErrors
					For Each [error] In validationResult.ValidationErrors
						If stringBuilder.Length > 0 Then
							stringBuilder.AppendLine()
						End If
						stringBuilder.Append([error].PropertyName & ": " & [error].ErrorMessage)
					Next [error]
				Next validationResult
				Return New DbException(stringBuilder.ToString(), entityValidationException)
			End If
			Return New DbException("An error has occurred while updating the database.", entityValidationException)
		End Function
	End Class
	Public Class DbException
		Inherits Exception

'INSTANT VB NOTE: The variable message was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable innerException was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal message_Conflict As String, ByVal innerException_Conflict As Exception)
			MyBase.New(message_Conflict, innerException_Conflict)
		End Sub
	End Class
End Namespace