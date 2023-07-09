using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using NeerCore.Exceptions;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Repositories;

namespace VoicerStudio.Application.Infrastructure.Repositories;

internal sealed class SpeakerRepository : ISpeakerRepository
{
    private const string TableName = "Speakers";

    private readonly GoogleSheetsAccessor _googleSheets;
    private readonly SpreadsheetsResource.ValuesResource _googleSheetValues;

    public SpeakerRepository(GoogleSheetsAccessor googleSheetsAccessor)
    {
        _googleSheets = googleSheetsAccessor;
        _googleSheetValues = googleSheetsAccessor.Service.Spreadsheets.Values;
    }


    public async Task<Speaker> GetByIdAsync(int id, string sheet)
    {
        return await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            var all = await GetAllAsync(sheet);
            var speaker = all.FirstOrDefault(x => x.Id == id);
            if (speaker is null) throw new NotFoundException<Speaker>();
            return speaker;
        });
    }

    public async Task<IEnumerable<Speaker>> GetAllAsync(string sheet)
    {
        return await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            var request = _googleSheetValues.Get(sheet, $"{TableName}!A2:C");
            var response = await request.ExecuteAsync();
            return MapFromValues(response.Values);
        });
    }

    public async Task UpdateAsync(IEnumerable<Speaker> speakers, string sheet)
    {
        await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            await RemoveAllAsync(sheet);
            var valueRange = new ValueRange { Values = speakers.Select(MapToValuesRow).ToArray() };
            var updateRequest = _googleSheetValues.Update(valueRange, sheet, $"{TableName}!A2:C");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();
        });
    }

    public async Task RemoveAllAsync(string sheet)
    {
        await _googleSheets.WrapRequestAsync(sheet, async () =>
        {
            var requestBody = new ClearValuesRequest();
            var removeRequest = _googleSheetValues.Clear(requestBody, sheet, $"{TableName}!A2:C");
            await removeRequest.ExecuteAsync();
        });
    }


    private static IEnumerable<Speaker> MapFromValues(IList<IList<object>> values)
    {
        var index = 1;
        foreach (var row in values)
        {
            index++;
            if (row.Count < 3
                || !int.TryParse(row[0].ToString(), out var id))
                continue;

            var speaker = new Speaker
            {
                Id = id,
                RowId = index,
                Name = row[1].ToString()!,
                Preset = row[2].ToString()!,
            };
            yield return speaker;
        }
    }

    private static IList<object> MapToValuesRow(Speaker speaker)
    {
        var row = new object[]
        {
            speaker.Id,
            speaker.Name,
            speaker.Preset
        };
        return row;
    }
}