﻿namespace TransactionStore.Core.Exceptions;

public class ForbiddenException(string message) : Exception(message)
{
}