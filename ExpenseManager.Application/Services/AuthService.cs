using AutoMapper;
using ExpenseManager.Application.DTOs;
using ExpenseManager.Application.Exceptions;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.Enums;

namespace ExpenseManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ConflictException("Email is already in use.");
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.User 
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role.ToString()),
            User = _mapper.Map<UserResponseDto>(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials.");
        }


        return new AuthResponseDto
        {
            Token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role.ToString()),
            User = _mapper.Map<UserResponseDto>(user)
        };
    }
}
