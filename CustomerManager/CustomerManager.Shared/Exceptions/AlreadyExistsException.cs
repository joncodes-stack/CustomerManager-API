namespace CustomerManager.Shared.Exceptions;

public class AlreadyExistsException(string message) : BusinessException(message);
