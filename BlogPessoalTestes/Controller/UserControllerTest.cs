using blogpessoal.Model;
using BlogPessoalTestes.Factory;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit.Extensions.Ordering;

namespace BlogPessoalTestes.Controller
{
    public class UserControllerTest : IClassFixture<WebAppFactory>
    {
        protected readonly WebAppFactory _factory;
        protected HttpClient _client;

        private readonly dynamic token;
        private string Id { get; set; } = string.Empty;

        public UserControllerTest(WebAppFactory factory)
        {
            _factory = factory; 
            _client = factory.CreateClient();

            token = GetToken();
        }

        private dynamic GetToken()
        {
            dynamic data = new ExpandoObject();
            data.sub = "root@root.com";
            return data;
        }

        [Fact, Order(1)]
        public async Task DeveCriarNovoUsuario()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                { "nome", "João" },
                { "usuario", "joao@gmail.com"},
                { "senha", "123456789" },
                { "foto", "_" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);

            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            var resposta = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            resposta.EnsureSuccessStatusCode();

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact, Order(2)]
        public async Task DeveDarErroEmail()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                { "nome", "João" },
                { "usuario", "joaogmail.com"},
                { "senha", "123456789" },
                { "foto", "_" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);

            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            var resposta = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact, Order(3)]
        public async Task NaoDeveCriarUsuarioDuplicado()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                { "nome", "João Pedro" },
                { "usuario", "joao@gmail.com"},
                { "senha", "123456" },
                { "foto", "" }
            };

            var usuarioJson = JsonConvert.SerializeObject(novoUsuario);

            var corpoRequisicao = new StringContent(usuarioJson, Encoding.UTF8, "application/json");

            await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            var resposta = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicao);

            resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact, Order(4)]
        public async Task DeveAtualizarUsuario()
        {
            var novoUsuario = new Dictionary<string, string>()
            {
                { "nome", "Pedro" },
                { "usuario", "pedro@gmail.com"},
                { "senha", "123456789" },
                { "foto", "" }
            };

            var postJson = JsonConvert.SerializeObject(novoUsuario);

            var corpoRequisicaoPost = new StringContent(postJson, Encoding.UTF8, "application/json");

            var respostaPost = await _client.PostAsync("/usuarios/cadastrar", corpoRequisicaoPost);

            var corpoRespostaPost = await respostaPost.Content.ReadFromJsonAsync<User>();

            if (corpoRespostaPost is not null)
                Id = corpoRespostaPost.Id.ToString();

            //------------------------------------------------------------------------------------------------------

            var atualizarUsuario = new Dictionary<string, string>()
             {
                { "id", Id },
                { "nome", "Pedro Maia" },
                { "usuario", "pedro@gmail.com"},
                { "senha", "12345678" },
                { "foto", "" }
            };

            var updateJson = JsonConvert.SerializeObject(atualizarUsuario);

            var corpoRequisicaoUpdate = new StringContent(updateJson, Encoding.UTF8, "application/json");

            _client.SetFakeBearerToken((object)token);

            var respostaPut = await _client.PutAsync("/usuarios/atualizar", corpoRequisicaoUpdate);

            respostaPut.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact, Order(5)]
        public async Task DeveListarTodosUsuarios()
        {
            _client.SetFakeBearerToken((object)token);

            var resposta = await _client.GetAsync("/usuarios/all");

            resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact, Order(6)]
        public async Task DeveListarUmUsuario()
        {
            _client.SetFakeBearerToken((object)token);

            var resposta = await _client.GetAsync("/usuarios/1");

            resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact, Order(7)]
        public async Task DeveAutenticarUmUsuario()
        {
            var login = new Dictionary<string, string>()
            {
                { "usuario", "joao@gmail.com"},
                { "senha", "123456789" },
             };

            var loginJson = JsonConvert.SerializeObject(login);

            var corpoRequisicao = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var resposta = await _client.PostAsync("/usuarios/logar", corpoRequisicao);

            resposta.EnsureSuccessStatusCode();

            resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
