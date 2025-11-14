// Nome do Arquivo: Program.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Importado
using SistemaChamadosApi.Data;     // Importado
using SistemaChamadosApi.Models;   // Importado

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
});

// 2. LER A CONNECTION STRING DO appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 3. REGISTRAR O DBCONTEXT (A PONTE COM O BANCO)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// 4. Pipeline
app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();

// 5. Mapeamento de Rotas
app.MapGroup("/api/auth").MapAuthApi();
app.MapGroup("/api/usuarios").MapUsuarioApi();
app.MapGroup("/api/produtos").MapProdutoApi();
app.MapGroup("/api/chamados").MapChamadoApi();

app.MapFallbackToFile("index.html");

app.Run();

// --- ENDPOINTS COM BANCO DE DADOS ---

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder app)
    {
        // Login
        app.MapPost("/login", async ([FromBody] LoginRequestDto login, AppDbContext db) =>
        {
            Console.WriteLine($"[API] Recebida tentativa de login para ID: {login.IdEmpresarial}");
            
            // Busca no banco de dados
            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(u => u.IdEmpresarial == login.IdEmpresarial && u.Senha == login.Senha);

            if (usuario != null)
            {
                Console.WriteLine($"[API] Login APROVADO.");
                // Retorna o nome do usuário do banco
                return Results.Ok(new { message = "Sucesso", nome = usuario.NomeUsuario });
            }

            Console.WriteLine($"[API] Login REJEITADO.");
            return Results.Json(new { message = "Inválido" }, statusCode: 401);
        });
        return app;
    }
}

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioApi(this IEndpointRouteBuilder app)
    {
        // Registrar
        app.MapPost("/registrar", async ([FromBody] UsuarioCadastroDto u, AppDbContext db) =>
        {
            // Converte o DTO para o modelo do PIM
            var novoUsuario = new Usuario
            {
                IdEmpresarial = u.IdEmpresarial,
                NomeUsuario = u.Nome,
                Email = u.Email,
                Senha = u.Senha,
                Cargo = "Funcionario" // Padrão
            };

            await db.Usuarios.AddAsync(novoUsuario);
            await db.SaveChangesAsync(); // Salva no banco

            Console.WriteLine($"[API] Novo usuário registrado: {u.Nome} (ID: {u.IdEmpresarial})");
            return Results.Ok(new { message = "Cadastrado!" });
        });
        return app;
    }
}

public static class ProdutoEndpoints
{
    public static IEndpointRouteBuilder MapProdutoApi(this IEndpointRouteBuilder app)
    {
        // Listar Produtos do Banco
        app.MapGet("/", async (AppDbContext db) =>
        {
            Console.WriteLine("[API] Requisição GET /api/produtos recebida.");
            
            var lista = await db.Produtos.ToListAsync(); // Busca todos do banco
            
            return Results.Ok(lista);
        });
        return app;
    }
}

public static class ChamadoEndpoints
{
    public static IEndpointRouteBuilder MapChamadoApi(this IEndpointRouteBuilder app)
    {
        // Listar Todos
        app.MapGet("/", async (AppDbContext db) =>
        {
            Console.WriteLine($"[API] GET /api/chamados. Buscando no banco...");
            
            // Busca do banco, incluindo o nome do Usuário (JOIN)
            var chamados = await db.SolicitacoesServico
                .Include(s => s.Usuario) // Faz o JOIN com a tabela USUARIO
                .Include(s => s.Servico) // Faz o JOIN com a tabela SERVICO
                .Select(s => new ChamadoDto // Converte para o DTO que o Front-end espera
                {
                    IdSolicitacao = s.IdSolicitacao,
                    Assunto = s.Servico != null ? s.Servico.DescricaoServico : "N/A",
                    Descricao = s.Descricao,
                    SolicitanteNome = s.Usuario != null ? s.Usuario.NomeUsuario : "N/A",
                    DataSolicitacao = s.DataSolicitacao,
                    Prioridade = s.Prioridade,
                    Status = s.Status
                })
                .ToListAsync();

            return Results.Ok(chamados);
        });

        // Obter Um (Para edição)
        app.MapGet("/{id}", async (int id, AppDbContext db) =>
        {
            Console.WriteLine($"[API] GET /api/chamados/{id}. Buscando chamado...");
            
            var s = await db.SolicitacoesServico
                .Include(s => s.Servico)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(x => x.IdSolicitacao == id);

            if (s == null) return Results.NotFound();

            // Converte para o DTO
            var dto = new ChamadoDto
            {
                IdSolicitacao = s.IdSolicitacao,
                Assunto = s.Servico != null ? s.Servico.DescricaoServico : "N/A",
                Descricao = s.Descricao,
                SolicitanteNome = s.Usuario != null ? s.Usuario.NomeUsuario : "N/A",
                DataSolicitacao = s.DataSolicitacao,
                Prioridade = s.Prioridade,
                Status = s.Status
            };
            
            return Results.Ok(dto);
        });

        // Criar Novo (Lógica adaptada ao PIM)
        app.MapPost("/", async ([FromBody] ChamadoRequestDto novo, AppDbContext db) =>
        {
            // No PIM, o "Assunto" é um "Serviço".
            // 1. Criamos o Serviço primeiro.
            var novoServico = new Servico
            {
                DescricaoServico = novo.Assunto, // Assunto vira a descrição do serviço
                IdUsuario = 1 // Simulado - Idealmente viria do usuário logado
            };
            await db.Servicos.AddAsync(novoServico);
            await db.SaveChangesAsync(); // Salva para obter o ID_SERVICO

            // 2. Criamos a Solicitação (Chamado)
            var novaSolicitacao = new SolicitacaoServico
            {
                IdServico = novoServico.IdServico, // Linka ao serviço criado
                IdUsuario = 1, // Simulado - Idealmente viria do usuário logado
                Descricao = novo.Descricao,
                Prioridade = novo.Prioridade,
                Status = "Pendente", // Padrão
                DataSolicitacao = DateTime.Now
            };
            await db.SolicitacoesServico.AddAsync(novaSolicitacao);
            await db.SaveChangesAsync(); // Salva a solicitação

            Console.WriteLine($"[API] POST /api/chamados. Novo chamado criado - ID: {novaSolicitacao.IdSolicitacao}");

            return Results.Ok(new { message = "Criado", id = novaSolicitacao.IdSolicitacao });
        });

        // Atualizar (Salvar Edição)
        app.MapPut("/{id}", async (int id, [FromBody] ChamadoDto atualizado, AppDbContext db) =>
        {
            Console.WriteLine($"[API] PUT /api/chamados/{id}. Atualizando chamado...");
            
            var existente = await db.SolicitacoesServico
                                    .Include(s => s.Servico) // Carrega o Serviço relacionado
                                    .FirstOrDefaultAsync(x => x.IdSolicitacao == id);
            
            if (existente == null) return Results.NotFound();

            // Atualiza os dados da Solicitação
            existente.Descricao = atualizado.Descricao;
            existente.Prioridade = atualizado.Prioridade;
            existente.Status = atualizado.Status;

            // Atualiza o Assunto (que está na tabela Serviço)
            if (existente.Servico != null)
            {
                existente.Servico.DescricaoServico = atualizado.Assunto;
            }

            await db.SaveChangesAsync(); // Salva todas as mudanças no banco

            return Results.Ok(new { message = "Atualizado" });
        });

        return app;
    }
}