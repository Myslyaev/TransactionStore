using AutoMapper;
using TransactionStore.Core.DTOs;
using TransactionStore.Core.Models.Requests;
using TransactionStore.Core.Models.Responses;

namespace TransactionStore.Core.Models;

public class TransactionsMappingProfile : Profile
{
    public TransactionsMappingProfile()
    {
        CreateMap<DepositWithdrawRequest, TransactionDto>();
        CreateMap<TransferRequest, TransactionDto>();

        CreateMap<TransactionDto, AccountBalanceResponse>();
        CreateMap<TransactionDto, TransactionResponse>();
    }
}