﻿namespace LoggingDecoratorGenerator.IntegrationTests;

[Decorate]
public interface ISomeService
{
    DateTime DateTimeReturningMethod(DateTime dateTime);
}