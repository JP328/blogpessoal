using blogpessoal.Model;
using blogpessoal.NovaPasta3;
using blogpessoal.Service;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blogpessoal.Controllers
{
    [ApiController]
    [Route("~/usuarios")]    
    public class UserController : ControllerBase
    {
        private readonly IUserService _usuarioService;
        private readonly IValidator<User> _usuarioValidator;
        private readonly IAuthService _authService;

        public UserController(IUserService usuarioService, IValidator<User> usuarioValidator, IAuthService authService)
        {
            _usuarioService = usuarioService;
            _usuarioValidator = usuarioValidator;
            _authService = authService;
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult> GetAll()
        {
            return Ok(await _usuarioService.GetAll());
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(long id)
        {
            var Resposta = await _usuarioService.GetById(id);

            if (Resposta is null)
                return NotFound();

            return Ok(Resposta);
        }

        [AllowAnonymous]
        [HttpPost("cadastrar")]
        public async Task<ActionResult> Create([FromBody] User usuario)
        {
            var ValidarUser = await _usuarioValidator.ValidateAsync(usuario);

            if (!ValidarUser.IsValid)
                return BadRequest(ValidarUser.Errors);

            var Resposta = await _usuarioService.Create(usuario);

            if (Resposta is null)
                return BadRequest("Usuário já está cadastrado");

            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
        }

        [AllowAnonymous]
        [HttpPost("logar")]
        public async Task<ActionResult> Autenticar([FromBody] UserLogin userLogin)
        {
            var Resposta = await _authService.Autenticar(userLogin);

            if (Resposta is null)
                return Unauthorized("Usuário ou/e senha inválidos");

            return Ok(Resposta);
        }

        [Authorize]
        [HttpPut("atualizar")]
        public async Task<ActionResult> Update([FromBody] User usuario)
        {
            if (usuario.Id == 0)
                return BadRequest("Id do Usuário Inválido");

            var ValidarUser = await _usuarioValidator.ValidateAsync(usuario);

            if (!ValidarUser.IsValid)
                return BadRequest(ValidarUser.Errors);

            var UserUpadate = await _usuarioService.GetByUser(usuario.Usuario);

            if (UserUpadate is not null && UserUpadate.Id != usuario.Id)
                return BadRequest("O Usuário (e-mail) já está em uso por outro usuário");

            var Resposta = await _usuarioService.Update(usuario);

            if (Resposta is null)
                return BadRequest("Usuário não encontrado");

            return Ok(Resposta);
        }
    }
}
