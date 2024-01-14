using FacturacionAPI1.Models;
using FacturacionAPI1.Models.Dto;
using FacturacionAPI1.Repository;
using AutoMapper;

namespace FacturacionAPI1
{
    public class MappingConfig : Profile
    {
        public MappingConfig() {

            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Usuario, UsuarioCreateDto>().ReverseMap();
            CreateMap<Usuario, UsuarioUpdateDto>().ReverseMap();

            CreateMap<Producto, ProductoDto>().ReverseMap();
            CreateMap<Producto, ProductoCreateDto>().ReverseMap();
            CreateMap<Producto, ProductoUpdateDto>().ReverseMap();

            CreateMap<FamiliaProducto, FamiliaProductoDto>().ReverseMap();
            CreateMap<FamiliaProducto, FamiliaProductoCreateDto>().ReverseMap();
            CreateMap<FamiliaProducto, FamiliaProductoUpdateDto>().ReverseMap();

            CreateMap<Factura, FacturaDto>().ReverseMap();
            CreateMap<Factura, FacturaCreateDto>().ReverseMap();
            CreateMap<Factura, FacturaUpdateDto>().ReverseMap();

            CreateMap<DetalleFactura, DetalleFacturaDto>().ReverseMap();
            CreateMap<DetalleFactura, DetalleFacturaCreateDto>().ReverseMap();
            CreateMap<DetalleFactura, DetalleFacturaUpdateDto>().ReverseMap();
        }
    }
}
